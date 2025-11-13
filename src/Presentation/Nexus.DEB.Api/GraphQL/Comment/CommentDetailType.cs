using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class CommentDetailType : ObjectType<CommentDetail>
    {
        private readonly ICurrentUserService _currentUserService;

        public CommentDetailType(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        protected override void Configure(IObjectTypeDescriptor<CommentDetail> descriptor)
        {
            descriptor.Field(x => x.Id).ID();

            descriptor
                .Field("isOwner")
                .Resolve(context =>
                {
                    CommentDetail parent = context.Parent<CommentDetail>();

                    return (parent.CreatedByPostId == _currentUserService.PostId);
                });

            base.Configure(descriptor);
        }
    }
}
