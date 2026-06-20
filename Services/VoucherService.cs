using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextService _httpContextService;
        private readonly IUploadFileService _uploadFileService;
        private readonly INotificationService _notificationService;

        public VoucherService(IUnitOfWork unitOfWork, IMapper mapper,
            IHttpContextService httpContextService, IUploadFileService uploadFileService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextService = httpContextService;
            _uploadFileService = uploadFileService;
            _notificationService = notificationService;
        }

        public async Task<bool> Create(CreateVoucherRequest request)
        {

            if (request.File == null || request.File.Length == 0)
            {
                throw new AppException("Không nhận được file");
            }

            List<string> usernames = new List<string>();
            using (var stream = new MemoryStream())
            {
                await request.File.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1);
                    foreach (var row in rows)
                    {
                        var username = row.Cell(1).GetString().Trim();
                        if (!string.IsNullOrEmpty(username))
                        {
                            usernames.Add(username);
                        }
                    }
                }
            }

            if (!usernames.Any())
            {
                throw new AppException("File không hợp lệ");
            }

            // Lấy danh sách AccountId từ Username
            var accounts = _unitOfWork.Repository<Account>().GetQueryable()
                .Where(a => usernames.Contains(a.Username))
                .Select(a => new { a.Id, a.Username })
                .ToList();

            if (!accounts.Any())
            {
                throw new AppException("Không tìm thấy khách hàng");
            }
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                var currentAccount = await _httpContextService.GetCurrentAccount();

                string voucherImage = await _uploadFileService.UploadImage(request.Image);
                // Tạo mới Voucher
                var voucher = new Voucher
                {
                    Name = request.Name,
                    Amount = request.Amount,
                    Image = voucherImage,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Description = request.Description,
                    Status = (int)EVoucherStatus.Active,
                };

                await _unitOfWork.Repository<Voucher>().Add(voucher, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();

                // Gán Voucher cho các Account từ danh sách Username
                var voucherAccounts = accounts.Select(a => new VoucherAccount
                {
                    AccountId = a.Id,
                    VoucherId = voucher.Id,
                    Name = voucher.Name,
                    Status = (int)EVoucherAccountStatus.New,
                    Image = voucher.Image,
                    Amount = voucher.Amount,
                    StartDate = voucher.StartDate,
                    EndDate = voucher.EndDate,
                    Description = voucher.Description,
                }).ToList();

                await _unitOfWork.Repository<VoucherAccount>().AddRange(voucherAccounts, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                foreach (var voucherAccount in voucherAccounts)
                {
                    var notification = new Notification
                    {
                        Title = "Voucher TPK",
                        Content = $"Nhận được voucher {voucher.Name}",
                        WebUrl = $"",
                        Type = (int)ENotificationType.Voucher,
                        IsStaff = false,
                    };
                    await _notificationService.SendNotification(notification, currentDate, currentAccount.Id,
                        customerId: voucherAccount.AccountId);
                }
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }

        public async Task<PagedList<VoucherResponse>> GetPaging(VoucherSearch voucherSearch)
        {
            var vouchers = await (from voucher in _unitOfWork.Repository<Voucher>().GetQueryable()
                                  join voucherAccount in _unitOfWork.Repository<VoucherAccount>().GetQueryable()
                                  on voucher.Id equals voucherAccount.VoucherId into voucherAccountJoin
                                  where (voucherSearch.Status == null || voucher.Status == voucherSearch.Status)
                                        && (voucherSearch.StartDate == null || voucher.StartDate >= voucherSearch.StartDate)
                                        && (voucherSearch.EndDate == null || voucher.EndDate <= voucherSearch.EndDate)
                                  select new VoucherResponse
                                  {
                                      Id = voucher.Id,
                                      Name = voucher.Name,
                                      Status = voucher.Status,
                                      Image = voucher.Image,
                                      Amount = voucher.Amount,
                                      StartDate = voucher.StartDate,
                                      EndDate = voucher.EndDate,
                                      Description = voucher.Description,
                                      Created = voucher.Created,
                                      CreatedBy = voucher.CreatedBy,
                                      Updated = voucher.Updated,
                                      UpdateBy = voucher.UpdateBy,
                                      Quantity = voucherAccountJoin.Count()
                                  })
                          .OrderByDescending(x => x.Id)
                          .Skip((voucherSearch.PageIndex - 1) * voucherSearch.PageSize)
                          .Take(voucherSearch.PageSize)
                          .ToListAsync();

            int total = await _unitOfWork.Repository<Voucher>().GetQueryable()
                            .Where(x => (voucherSearch.Status == null || x.Status == voucherSearch.Status)
                                     && (voucherSearch.StartDate == null || x.StartDate >= voucherSearch.StartDate)
                                     && (voucherSearch.EndDate == null || x.EndDate <= voucherSearch.EndDate))
                            .CountAsync();

            return new PagedList<VoucherResponse>
            {
                PageIndex = voucherSearch.PageIndex,
                PageSize = voucherSearch.PageSize,
                TotalItem = total,
                Items = vouchers
            };
        }

        public async Task<List<VoucherAccount>> GetVoucherAccountByCurrentAccountId()
        {
            var currentAccount = await _httpContextService.GetCurrentAccount();

            return await _unitOfWork.Repository<VoucherAccount>().GetQueryable()
                .Where(x => x.AccountId == currentAccount.Id && x.Status == (int)EVoucherAccountStatus.New
                    && x.StartDate.Date <= DateTime.Now.Date && x.EndDate.Date >= DateTime.Now.Date
                )
                .ToListAsync();
        }

        public async Task<VoucherAccountDetail> GetVoucherAccountDetail(int id)
        {
            var voucher = await _unitOfWork.Repository<Voucher>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            var quantity = await _unitOfWork.Repository<VoucherAccount>().GetQueryable().Where(x => x.VoucherId == voucher.Id).CountAsync();
            var usedQuantity = await _unitOfWork.Repository<VoucherAccount>().GetQueryable().Where(x => x.VoucherId == voucher.Id && x.Status == (int)EVoucherAccountStatus.Used).CountAsync();
            return new VoucherAccountDetail { Id = id, Quantity = quantity, UsedQuantity = usedQuantity };
        }

        public async Task<PagedList<VoucherAccountResponse>> GetVoucherAccountPaging(VoucherAccountSearch search)
        {
            var voucherAccounts = await (from voucherAccount in _unitOfWork.Repository<VoucherAccount>().GetQueryable()
                                         join account in _unitOfWork.Repository<Account>().GetQueryable() on voucherAccount.AccountId equals account.Id into accountJoin
                                         from account in accountJoin.DefaultIfEmpty()
                                         where voucherAccount.VoucherId == search.Id
                                            && (search.Status == null || voucherAccount.Status == search.Status)
                                            && (search.AccountId == null || voucherAccount.AccountId == search.AccountId)
                                         select new VoucherAccountResponse
                                         {
                                             Id = voucherAccount.Id,
                                             Name = voucherAccount.Name,
                                             Status = voucherAccount.Status,
                                             Image = voucherAccount.Image,
                                             Amount = voucherAccount.Amount,
                                             StartDate = voucherAccount.StartDate,
                                             EndDate = voucherAccount.EndDate,
                                             Description = voucherAccount.Description,
                                             Created = voucherAccount.Created,
                                             CreatedBy = voucherAccount.CreatedBy,
                                             Updated = voucherAccount.Updated,
                                             UpdateBy = voucherAccount.UpdateBy,
                                             Username = account != null ? account.Username : string.Empty,
                                         })
              .OrderBy(x => x.Username)
              .Skip((search.PageIndex - 1) * search.PageSize)
              .Take(search.PageSize)
              .ToListAsync();

            int total = await _unitOfWork.Repository<VoucherAccount>().GetQueryable()
                            .Where(x => x.Id == search.Id
                                    && (search.Status == null || x.Status == search.Status)
                                    && (search.AccountId == null || x.AccountId == search.AccountId))
                            .CountAsync();

            return new PagedList<VoucherAccountResponse>
            {
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total,
                Items = voucherAccounts
            };
        }

        public async Task<bool> RecallAccountVoucher(int id)
        {
            var voucherAccount = await _unitOfWork.Repository<VoucherAccount>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            _unitOfWork.Repository<VoucherAccount>().Delete(voucherAccount);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> Update(int id, UpdateVoucherRequest request)
        {
            // Kiểm tra voucher có tồn tại không
            var voucher = await _unitOfWork.Repository<Voucher>().GetQueryable().FirstOrDefaultAsync(x => x.Id == id);
            if (voucher == null)
            {
                throw new AppException("Voucher không tồn tại");
            }

            List<string> usernames = new List<string>();

            if (request.File != null && request.File.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    await request.File.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RowsUsed().Skip(1);
                        foreach (var row in rows)
                        {
                            var username = row.Cell(1).GetString().Trim();
                            if (!string.IsNullOrEmpty(username))
                            {
                                usernames.Add(username);
                            }
                        }
                    }
                }

                if (!usernames.Any())
                {
                    throw new AppException("File không hợp lệ");
                }
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                var currentAccount = await _httpContextService.GetCurrentAccount();

                // Cập nhật hình ảnh nếu có
                if (request.Image != null)
                {
                    voucher.Image = await _uploadFileService.UploadImage(request.Image);
                }

                // Cập nhật thông tin voucher
                voucher.Name = request.Name;
                voucher.Amount = request.Amount;
                voucher.StartDate = request.StartDate;
                voucher.EndDate = request.EndDate;
                voucher.Description = request.Description;
                voucher.Status = request.Status;

                _unitOfWork.Repository<Voucher>().Update(voucher, currentDate, currentAccount.Id);

                var updateVoucherAccounts = await _unitOfWork.Repository<VoucherAccount>().GetQueryable().Where(x => x.VoucherId == voucher.Id).ToListAsync();
                if (updateVoucherAccounts.Any())
                {
                    foreach (var item in updateVoucherAccounts)
                    {
                        item.Name = voucher.Name;
                        item.Amount = voucher.Amount;
                        item.StartDate = voucher.StartDate;
                        item.EndDate = voucher.EndDate;
                        item.Description = voucher.Description;
                        item.Image = voucher.Image;
                    }
                    _unitOfWork.Repository<VoucherAccount>().UpdateRange(updateVoucherAccounts, currentDate, currentAccount.Id);
                }
                await _unitOfWork.SaveAsync();
                // Nếu có danh sách tài khoản mới từ file, cập nhật danh sách người nhận voucher
                if (usernames.Any())
                {
                    var accounts = _unitOfWork.Repository<Account>().GetQueryable()
                        .Where(a => usernames.Contains(a.Username))
                        .Select(a => new { a.Id, a.Username })
                        .ToList();

                    if (!accounts.Any())
                    {
                        throw new AppException("Không tìm thấy khách hàng");
                    }

                    // Lấy danh sách VoucherAccount hiện tại, trừ những cái có trạng thái Used
                    var existingVoucherAccounts = _unitOfWork.Repository<VoucherAccount>()
                        .GetQueryable()
                        .Where(va => va.VoucherId == voucher.Id && va.Status != (int)EVoucherAccountStatus.Used)
                        .ToList();

                    // Xóa danh sách người nhận cũ (trừ những cái có trạng thái Used)
                    _unitOfWork.Repository<VoucherAccount>().DeleteRange(existingVoucherAccounts);
                    await _unitOfWork.SaveAsync();

                    // Lọc ra những AccountId đã tồn tại trong VoucherAccount (trừ những cái có trạng thái Used)
                    var existingAccountIds = existingVoucherAccounts.Select(va => va.AccountId).ToList();

                    // Thêm danh sách người nhận mới, trừ những khách hàng đã tồn tại
                    var voucherAccounts = accounts
                        .Where(a => !existingAccountIds.Contains(a.Id)) // Chỉ thêm những khách hàng chưa tồn tại
                        .Select(a => new VoucherAccount
                        {
                            AccountId = a.Id,
                            VoucherId = voucher.Id,
                            Name = voucher.Name,
                            Status = (int)EVoucherAccountStatus.New,
                            Image = voucher.Image,
                            Amount = voucher.Amount,
                            StartDate = voucher.StartDate,
                            EndDate = voucher.EndDate,
                            Description = voucher.Description,
                        }).ToList();

                    await _unitOfWork.Repository<VoucherAccount>().AddRange(voucherAccounts, currentDate, currentAccount.Id);
                    await _unitOfWork.SaveAsync();
                    foreach (var voucherAccount in voucherAccounts)
                    {
                        var notification = new Notification
                        {
                            Title = "Voucher TPK",
                            Content = $"Nhận được voucher {voucher.Name}",
                            WebUrl = $"",
                            Type = (int)ENotificationType.Voucher,
                            IsStaff = false
                        };
                        await _notificationService.SendNotification(notification, currentDate, currentAccount.Id,
                            customerId: voucherAccount.AccountId);
                    }
                }
                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }
    }
}
