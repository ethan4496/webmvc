using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using ClosedXML.Excel;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;
using System.Text.Json;

namespace WebMVC.Services
{
    public class PricingImportService : IPricingImportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextService _httpContextService;
        public PricingImportService(IUnitOfWork unitOfWork, IHttpContextService httpContextService)
        {
            _unitOfWork = unitOfWork;
            _httpContextService = httpContextService;
        }
        public async Task ImportExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("File không hợp lệ");
            }
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();

            using var stream = new MemoryStream();

            await file.CopyToAsync(stream);

            using var workbook = new XLWorkbook(stream);

            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RowsUsed().Skip(1);
            var priceImports = new List<PriceImport>();
            var codes = _unitOfWork.Repository<PriceImport>().GetQueryable();
            foreach (var row in rows)
            {
                var hsCode = row.Cell(3).GetString();
                var name = row.Cell(2).GetString();
                var taxVAT = row.Cell(4).GetValue<decimal>();
                var taxNK = row.Cell(5).GetValue<decimal>();
                var price = row.Cell(6).GetString();
                var origin = row.Cell(7).GetString();
                var policy = row.Cell(8).GetString().Trim() == "1";
                bool exists = codes.Any(x => x.Hscode == hsCode) || priceImports.Any(x => x.Hscode == hsCode);
                Console.WriteLine(
                    row.Cell(8).GetString().Trim()
                );
                Console.WriteLine(
                    JsonSerializer.Serialize(
                        policy,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }
                    )
                );
                if(exists) continue;
                var priceImport = new PriceImport()
                {
                    Name = name,
                    Hscode = hsCode,
                    TaxVAT = taxVAT,
                    TaxNK = taxNK,
                    Price = price,
                    Origin = origin,
                    Policy = policy
                };
                priceImports.Add(priceImport);
                await _unitOfWork.Repository<PriceImport>().Add(priceImport, currentDate, currentAccount.Id);
            }
            await _unitOfWork.SaveAsync();
        }
        public async Task<PagedResult<PriceImport>> GetListAsync(PriceImportFilter filter)
        {
            var query = _unitOfWork.Repository<PriceImport>().GetQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var keywords = filter.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var keyword in keywords)
                {
                    query = query.Where(x => x.Name.Contains(keyword));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Hscode))
            {
                query = query.Where(x => x.Hscode == filter.Hscode);
            }
            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // return new PagedResult<PriceImport>
            // {
            //     Items = items,
            //     TotalItems = totalItems,
            //     CurrentPage = filter.Page,
            //     PageSize = filter.PageSize
            // };
            return new PagedResult<PriceImport>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task UpdateAsync(PriceImport model)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var entity = await _unitOfWork.Repository<PriceImport>().GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (entity == null)
            {
                throw new Exception("Không tìm thấy dữ liệu");
            }

            entity.Name = model.Name;
            entity.Hscode = model.Hscode;
            entity.TaxVAT = model.TaxVAT;
            entity.TaxNK = model.TaxNK;
            entity.Price = model.Price;
            entity.Origin = model.Origin;
            entity.Policy = model.Policy;

            entity.Updated = DateTime.Now;
            entity.UpdateBy = currentAccount.Id;

            _unitOfWork.Repository<PriceImport>().Update(entity, currentDate, currentAccount.Id);

            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PriceImport>().GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                throw new Exception("Không tìm thấy dữ liệu");
            }

            _unitOfWork.Repository<PriceImport>().Delete(entity);

            await _unitOfWork.SaveAsync();
        }
    }
}