using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextService _httpContextService;
        private readonly IUploadFileService _uploadFileService;

        public PostService(IUnitOfWork unitOfWork,
            IHttpContextService httpContextService,
            IUploadFileService uploadFileService)
        {
            _unitOfWork = unitOfWork;
            _httpContextService = httpContextService;
            _uploadFileService = uploadFileService;
        }

        public async Task<PagedList<PostResponse>> GetPaging(PostSearch search)
        {
            var query = _unitOfWork.Repository<Post>().GetQueryable();
            if (!string.IsNullOrWhiteSpace(search.Title))
            {
                query = query.Where(x => x.Title.Contains(search.Title));
            }

            var total = await query.CountAsync();

            var posts = await query
                .Include(x => x.PostCategories).ThenInclude(pc => pc.Category)
                .OrderByDescending(x => x.Created)
                .Skip((search.PageIndex - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();

            var creatorIds = posts.Select(x => x.CreatedBy).Distinct().ToList();
            var authorNames = await _unitOfWork.Repository<Account>().GetQueryable()
                .Where(x => creatorIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.FullName);

            var items = posts.Select(x => new PostResponse
            {
                Id = x.Id,
                Title = x.Title,
                Excerpt = x.Excerpt,
                Content = x.Content,
                Image = x.Image,
                Categories = x.PostCategories
                    .Select(pc => new CategoryResponse { Id = pc.Category.Id, Name = pc.Category.Name })
                    .ToList(),
                AuthorName = authorNames.GetValueOrDefault(x.CreatedBy),
                Created = x.Created,
                CreatedBy = x.CreatedBy,
                Updated = x.Updated,
                UpdateBy = x.UpdateBy
            }).ToList();

            return new PagedList<PostResponse>
            {
                Items = items,
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total
            };
        }
        public async Task CreateAsync(CreatePostRequest request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var post = new Post
            {
                Title = request.Title,
                Slug = request.Slug,
                Excerpt = request.Excerpt,
                Content = request.Content,
                Created = currentDate,
            };
            if (request.Image != null)
            {
                post.Image = await _uploadFileService.UploadImage(request.Image);
            }
            // Console.WriteLine(
            //     "campagin"
            // );
            // Console.WriteLine(
            //     JsonConvert.SerializeObject(campagin, Formatting.Indented)
            // );

            await _unitOfWork.Repository<Post>().Add(post, currentDate, currentAccount.Id);

            await _unitOfWork.SaveAsync();

            if (request.CategoryIds != null && request.CategoryIds.Count > 0)
            {
                var postCategories = request.CategoryIds
                    .Distinct()
                    .Select(categoryId => new PostCategory { PostId = post.Id, CategoryId = categoryId })
                    .ToList();

                await _unitOfWork.PlainRepository<PostCategory>().AddRangeAsync(postCategories);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task<PostResponse> GetPostById(int id)
        {
            var post = await _unitOfWork.Repository<Post>().GetQueryable()
                .Include(x => x.PostCategories).ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            return await MapToResponse(post);
        }

        public async Task<PostResponse> GetPostBySlug(string slug)
        {
            var post = await _unitOfWork.Repository<Post>().GetQueryable()
                .Include(x => x.PostCategories).ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(x => x.Slug == slug);

            return await MapToResponse(post);
        }

        public async Task SaveAsync(int id, UpdatePostRequest request)
        {
            var post = await _unitOfWork.Repository<Post>().GetQueryable()
                .Include(x => x.PostCategories)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (post == null)
            {
                throw new Exception("Không tìm thấy bài viết");
            }

            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();

            post.Title = request.Title;
            post.Slug = request.Slug;
            post.Excerpt = request.Excerpt;
            post.Content = request.Content;

            if (request.Image != null)
            {
                post.Image = await _uploadFileService.UploadImage(request.Image);
            }

            _unitOfWork.Repository<Post>().Update(post, currentDate, currentAccount.Id);

            var newCategoryIds = (request.CategoryIds ?? new List<int>()).Distinct().ToList();
            var currentCategoryIds = post.PostCategories.Select(pc => pc.CategoryId).ToList();

            var toRemove = post.PostCategories.Where(pc => !newCategoryIds.Contains(pc.CategoryId)).ToList();
            if (toRemove.Count > 0)
            {
                _unitOfWork.PlainRepository<PostCategory>().RemoveRange(toRemove);
            }

            var toAdd = newCategoryIds
                .Where(categoryId => !currentCategoryIds.Contains(categoryId))
                .Select(categoryId => new PostCategory { PostId = post.Id, CategoryId = categoryId })
                .ToList();
            if (toAdd.Count > 0)
            {
                await _unitOfWork.PlainRepository<PostCategory>().AddRangeAsync(toAdd);
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var post = await _unitOfWork.Repository<Post>().GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (post == null)
            {
                throw new Exception("Không tìm thấy bài viết");
            }

            _unitOfWork.Repository<Post>().Delete(post);
            await _unitOfWork.SaveAsync();
        }

        private async Task<PostResponse> MapToResponse(Post post)
        {
            if (post == null)
            {
                return null;
            }

            var authorName = await _unitOfWork.Repository<Account>().GetQueryable()
                .Where(x => x.Id == post.CreatedBy)
                .Select(x => x.FullName)
                .FirstOrDefaultAsync();

            return new PostResponse
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Excerpt = post.Excerpt,
                Content = post.Content,
                Image = post.Image,
                Categories = post.PostCategories
                    .Select(pc => new CategoryResponse { Id = pc.Category.Id, Name = pc.Category.Name })
                    .ToList(),
                AuthorName = authorName,
                Created = post.Created,
                CreatedBy = post.CreatedBy,
                Updated = post.Updated,
                UpdateBy = post.UpdateBy
            };
        }
    }
}
