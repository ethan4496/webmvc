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
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextService _httpContextService;
        private readonly IUploadFileService _uploadFileService;

        public CategoryService(IUnitOfWork unitOfWork,
            IHttpContextService httpContextService,
            IUploadFileService uploadFileService)
        {
            _unitOfWork = unitOfWork;
            _httpContextService = httpContextService;
            _uploadFileService = uploadFileService;
        }

        public async Task<PagedList<CategoryResponse>> GetPaging(CategorySearch search)
        {
            var query = _unitOfWork.Repository<Category>().GetQueryable();
            if (!string.IsNullOrWhiteSpace(search.Name))
            {
                query = query.Where(x => x.Name.Contains(search.Name));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Created)
                .Skip((search.PageIndex - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(x => new CategoryResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Image = x.Image,
                    ParentId = x.ParentId,
                    ParentName = x.Parent != null ? x.Parent.Name : null,
                    Created = x.Created,
                    CreatedBy = x.CreatedBy,
                    Updated = x.Updated,
                    UpdateBy = x.UpdateBy
                })
                .ToListAsync();

            return new PagedList<CategoryResponse>
            {
                Items = items,
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total
            };
        }

        public async Task<Category> GetCategoryById(int id)
        {
            return await _unitOfWork.Repository<Category>().GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<object>> GetAllCategoryNames()
        {
            return await _unitOfWork.Repository<Category>().GetQueryable()
                .OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name })
                .Cast<object>()
                .ToListAsync();
        }

        public async Task CreateAsync(CreateCategoryRequest request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();

            if (request.ParentId.HasValue)
            {
                var parentExists = await _unitOfWork.Repository<Category>().GetQueryable()
                    .AnyAsync(x => x.Id == request.ParentId.Value);
                if (!parentExists)
                {
                    throw new Exception("Danh mục cha không tồn tại");
                }
            }

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                ParentId = request.ParentId,
            };

            if (request.Image != null)
            {
                category.Image = await _uploadFileService.UploadImage(request.Image);
            }

            await _unitOfWork.Repository<Category>().Add(category, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
        }

        public async Task SaveAsync(int id, UpdateCategoryRequest request)
        {
            var category = await _unitOfWork.Repository<Category>().GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
            {
                throw new Exception("Không tìm thấy danh mục");
            }

            if (request.ParentId.HasValue)
            {
                if (request.ParentId.Value == id)
                {
                    throw new Exception("Danh mục không thể là cha của chính nó");
                }
                var parentExists = await _unitOfWork.Repository<Category>().GetQueryable()
                    .AnyAsync(x => x.Id == request.ParentId.Value);
                if (!parentExists)
                {
                    throw new Exception("Danh mục cha không tồn tại");
                }
            }

            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();

            category.Name = request.Name;
            category.Description = request.Description;
            category.ParentId = request.ParentId;

            if (request.Image != null)
            {
                category.Image = await _uploadFileService.UploadImage(request.Image);
            }

            _unitOfWork.Repository<Category>().Update(category, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
            {
                throw new Exception("Không tìm thấy danh mục");
            }

            var hasChildren = await _unitOfWork.Repository<Category>().GetQueryable()
                .AnyAsync(x => x.ParentId == id);
            if (hasChildren)
            {
                throw new Exception("Không thể xóa danh mục đang có danh mục con");
            }

            var hasPosts = await _unitOfWork.Repository<Post>().GetQueryable()
                .AnyAsync(x => x.PostCategories.Any(pc => pc.CategoryId == id));
            if (hasPosts)
            {
                throw new Exception("Không thể xóa danh mục đang có bài viết");
            }

            _unitOfWork.Repository<Category>().Delete(category);
            await _unitOfWork.SaveAsync();
        }
    }
}
