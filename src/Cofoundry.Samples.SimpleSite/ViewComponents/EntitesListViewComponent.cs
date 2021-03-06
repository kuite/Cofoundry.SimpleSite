﻿using Cofoundry.Core;
using Cofoundry.Domain;
using Cofoundry.Web;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cofoundry.Samples.SimpleSite
{
    public class EntitiesListViewComponent : ViewComponent
    {
        private readonly ICustomEntityRepository _customEntityRepository;
        private readonly IImageAssetRepository _imageAssetRepository;
        private readonly IVisualEditorStateService _visualEditorStateService;

        public EntitiesListViewComponent(
            ICustomEntityRepository customEntityRepository,
            IImageAssetRepository imageAssetRepository,
            IVisualEditorStateService visualEditorStateService
            )
        {
            _customEntityRepository = customEntityRepository;
            _imageAssetRepository = imageAssetRepository;
            _visualEditorStateService = visualEditorStateService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var webQuery = ModelBind();

            // We can use the current visual editor state (e.g. edit mode, live mode) to
            // determine whether to show unpublished blog posts in the list.
            var visualEditorState = await _visualEditorStateService.GetCurrentAsync();
            var ambientEntityPublishStatusQuery = visualEditorState.GetAmbientEntityPublishStatusQuery();

            var query = new SearchCustomEntityRenderSummariesQuery()
            {
                CustomEntityDefinitionCode = EntityDefinition.DefinitionCode,
                PageNumber = webQuery.PageNumber,
                PageSize = 30,
                PublishStatus = ambientEntityPublishStatusQuery
            };

            // TODO: Filtering by Category (webQuery.CategoryId)
            // Searching/filtering custom entities is not implemented yet, but it
            // is possible to build your own search index using the message handling
            // framework or writing a custom query against the UnstructuredDataDependency table
            // See issue https://github.com/cofoundry-cms/cofoundry/issues/12

            var entities = await _customEntityRepository.SearchCustomEntityRenderSummariesAsync(query);
            List<Entity> listEnt = new List<Entity>();
            foreach (var e in entities.Items)
            {
                var castedModel = (EntityDataModel) e.Model;
                var ent = new Entity();
                ent.Title = e.Title;
                ent.Description = castedModel.Description;
                listEnt.Add(ent);
            }
            // var viewModel = await MapBlogPostsAsync(entities, ambientEntityPublishStatusQuery);

            return View(listEnt);
        }

        /// <summary>
        /// ModelBinder is not supported in view components so we have to bind
        /// this manually. We have an issue open to try and improve the experience here
        /// https://github.com/cofoundry-cms/cofoundry/issues/125
        /// </summary>
        private SearchBlogPostsQuery ModelBind()
        {
            var webQuery = new SearchBlogPostsQuery();
            webQuery.PageNumber = IntParser.ParseOrDefault(Request.Query[nameof(webQuery.PageNumber)]);
            webQuery.PageSize = IntParser.ParseOrDefault(Request.Query[nameof(webQuery.PageSize)]);
            webQuery.CategoryId = IntParser.ParseOrDefault(Request.Query[nameof(webQuery.CategoryId)]);

            return webQuery;
        }

        /// <summary>
        /// Here we map the raw custom entity data from Cofoundry into our
        /// own BlogPostSummary which will get sent to be rendered in the 
        /// view.
        /// 
        /// This code is repeated in HomepageBlogPostsViewComponent for 
        /// simplicity, but typically you'd put the code into a shared 
        /// mapper or break data access out into it's own shared layer.
        /// </summary>
        private async Task<PagedQueryResult<BlogPostSummary>> MapBlogPostsAsync(
            PagedQueryResult<CustomEntityRenderSummary> customEntityResult,
            PublishStatusQuery ambientEntityPublishStatusQuery
            )
        {
            var blogPosts = new List<BlogPostSummary>(customEntityResult.Items.Count());

            var imageAssetIds = customEntityResult
                .Items
                .Select(i => (BlogPostDataModel)i.Model)
                .Select(m => m.ThumbnailImageAssetId)
                .Distinct();

            var authorIds = customEntityResult
                .Items
                .Select(i => (BlogPostDataModel)i.Model)
                .Select(m => m.AuthorId)
                .Distinct();

            var imageLookup = await _imageAssetRepository.GetImageAssetRenderDetailsByIdRangeAsync(imageAssetIds);
            var authorQuery = new GetCustomEntityRenderSummariesByIdRangeQuery(authorIds, ambientEntityPublishStatusQuery);
            var authorLookup = await _customEntityRepository.GetCustomEntityRenderSummariesByIdRangeAsync(authorQuery);

            foreach (var customEntity in customEntityResult.Items)
            {
                var model = (BlogPostDataModel)customEntity.Model;

                var blogPost = new BlogPostSummary();
                blogPost.Title = customEntity.Title;
                blogPost.ShortDescription = model.ShortDescription;
                blogPost.ThumbnailImageAsset = imageLookup.GetOrDefault(model.ThumbnailImageAssetId);
                blogPost.FullPath = customEntity.PageUrls.FirstOrDefault();
                blogPost.PostDate = customEntity.PublishDate;

                var author = authorLookup.GetOrDefault(model.AuthorId);
                if (author != null)
                {
                    blogPost.AuthorName = author.Title;
                }

                blogPosts.Add(blogPost);
            }

            return customEntityResult.ChangeType(blogPosts);
        }
    }
}
