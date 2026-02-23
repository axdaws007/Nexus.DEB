using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public class SectionDomainService : ISectionDomainService
    {
        private readonly IDebService _debService;

        public SectionDomainService(IDebService debService)
        {
            _debService = debService;
        }

        public async Task<Result> MoveSectionAsync(Guid sectionId, Guid? newParentSectionId, int newOrdinal, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Load the section being moved
                var section = await _debService.GetSectionByIdAsync(sectionId, cancellationToken);

                if (section is null)
                    return Result.Failure($"Section '{sectionId}' was not found.");

                var oldParentSectionId = section.ParentSectionId;
                var oldOrdinal = section.Ordinal;
                var standardVersionId = section.StandardVersionId;

                bool parentChanged = oldParentSectionId != newParentSectionId;

                // 2. Validate: ensure new parent (if provided) belongs to the same StandardVersion
                //    and isn't a descendant of the section being moved (would create a cycle)
                if (newParentSectionId.HasValue)
                {
                    var newParent = await _debService.GetSectionByIdAsync(newParentSectionId.Value, cancellationToken);

                    if (newParent is null)
                        return Result.Failure($"Parent section '{newParentSectionId}' was not found.");

                    if (newParent.StandardVersionId != standardVersionId)
                        return Result.Failure("The parent section does not belong to the same standard version.");

                    var isDescendant = await _debService.IsSectionDescendantOfAsync(newParentSectionId.Value, sectionId, cancellationToken);

                    if (isDescendant)
                        return Result.Failure("Cannot move a section to one of its own descendants.");
                }

                // 3. Load siblings at the NEW parent location (excluding the moving section)
                var newSiblings = await _debService.GetSiblingSectionsAsync(standardVersionId, newParentSectionId, excludeSectionId: sectionId, cancellationToken);

                // 4. Validate ordinal range (1-based, max is count+1 to allow appending at end)
                var maxOrdinal = newSiblings.Count + 1;

                if (newOrdinal < 1 || newOrdinal > maxOrdinal)
                    return Result.Failure($"Ordinal must be between 1 and {maxOrdinal}.");

                var sectionsToUpdate = new List<Section>();

                if (parentChanged)
                {
                    // 5a. Close the gap at the OLD parent — shift down siblings above the old ordinal
                    var oldSiblings = await _debService.GetSiblingSectionsAsync(standardVersionId, oldParentSectionId, excludeSectionId: sectionId, cancellationToken);

                    foreach (var sibling in oldSiblings.Where(s => s.Ordinal > oldOrdinal))
                    {
                        sibling.Ordinal--;
                        sectionsToUpdate.Add(sibling);
                    }

                    // 5b. Open a slot at the NEW parent — shift up siblings at or above new ordinal
                    foreach (var sibling in newSiblings.Where(s => s.Ordinal >= newOrdinal))
                    {
                        sibling.Ordinal++;
                        sectionsToUpdate.Add(sibling);
                    }
                }
                else
                {
                    // 5c. Same parent — re-sequence siblings between the old and new ordinal positions
                    if (newOrdinal < oldOrdinal)
                    {
                        // Moving up: shift affected siblings down by 1
                        foreach (var sibling in newSiblings.Where(s => s.Ordinal >= newOrdinal && s.Ordinal < oldOrdinal))
                        {
                            sibling.Ordinal++;
                            sectionsToUpdate.Add(sibling);
                        }
                    }
                    else if (newOrdinal > oldOrdinal)
                    {
                        // Moving down: shift affected siblings up by 1
                        foreach (var sibling in newSiblings.Where(s => s.Ordinal > oldOrdinal && s.Ordinal <= newOrdinal))
                        {
                            sibling.Ordinal--;
                            sectionsToUpdate.Add(sibling);
                        }
                    }
                    else
                    {
                        // No change
                        return Result.Success();
                    }
                }

                // 6. Update the moved section itself
                section.ParentSectionId = newParentSectionId;
                section.Ordinal = newOrdinal;
                section.LastModifiedDate = DateTime.UtcNow;
                sectionsToUpdate.Add(section);

                // 7. Persist all changes in a single batch
                await _debService.UpdateSectionsAsync(sectionsToUpdate, cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occurred moving the Section: {ex.Message}");
            }
        }

        public async Task<Result<Section>> CreateSectionAsync(
            string reference,
            string title,
            bool displayReference,
            bool displayTitle,
            Guid? parentId,
            Guid standardVersionId,
            CancellationToken cancellationToken)
        {
            try
            {
                var ordinal = 1;

                var siblings = await _debService.GetSiblingSectionsAsync(standardVersionId, parentId, null, cancellationToken);

                if (siblings != null && siblings.Count > 0)
                {
                    ordinal = siblings.Last().Ordinal + 1;
                }

                var section = new Section()
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    IsReferenceDisplayed = displayReference,
                    IsTitleDisplayed = displayTitle,
                    LastModifiedDate = DateTime.Now,
                    Ordinal = ordinal,
                    ParentSectionId = parentId,
                    Reference = reference,
                    StandardVersionId = standardVersionId,
                    Title = title
                };

                await _debService.CreateSectionAsync(section, cancellationToken);

                return Result<Section>.Success(section);
            }
            catch (Exception ex)
            {
                return Result<Section>.Failure($"An error occurred creating the Section: {ex.Message}");
            }
        }

        public async Task<Result<Section>> UpdateSectionAsync(
            Guid sectionId,
            string reference,
            string title,
            bool displayReference,
            bool displayTitle,
            CancellationToken cancellationToken)
        {
            try
            {
                var section = await _debService.GetSectionAsync(sectionId, cancellationToken);

                if (section == null)
                {
                    return Result<Section>.Failure(new ValidationError()
                    {
                        Code = "INVALID_SECTION_ID",
                        Field = nameof(sectionId),
                        Message = "Section does not exist"
                    });
                }

                section.IsReferenceDisplayed = displayReference;
                section.IsTitleDisplayed = displayTitle;
                section.LastModifiedDate = DateTime.Now;
                section.Reference = reference;
                section.Title = title;

                await _debService.UpdateSectionAsync(section, cancellationToken);

                return Result<Section>.Success(section);
            }
            catch (Exception ex)
            {
                return Result<Section>.Failure($"An error occurred updating the Section: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteSectionAsync(Guid sectionId, CancellationToken cancellationToken)
        {
            try
            {
                var section = await _debService.GetSectionByIdAsync(sectionId, cancellationToken);

                if (section == null)
                {
                    return Result<bool>.Failure(new ValidationError()
                    {
                        Code = "INVALID_SECTION_ID",
                        Field = nameof(sectionId),
                        Message = "Section does not exist"
                    });
                }

                var children = await _debService.GetSiblingSectionsAsync(section.StandardVersionId, section.Id, null, cancellationToken);

                if (children != null && children.Count > 0)
                {
                    return Result<bool>.Failure(new ValidationError()
                    {
                        Code = "SECTION_HAS_CHILDREN",
                        Field = nameof(sectionId),
                        Message = "The section has children and cannot be deleted."
                    });
                }

                var isDeleted = await _debService.DeleteSectionByIdAsync(section.Id, cancellationToken);

                return Result<bool>.Success(isDeleted);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred deleting the Section: {ex.Message}");
            }
        }
    }
}
