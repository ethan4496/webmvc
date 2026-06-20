using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using System.Text.Json;
using WebMVC.Ultilities.Enums;
namespace WebMVC.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextService _httpContextService;
        private readonly ISendEmailService _sendEmailService;
        private readonly INotificationService _notificationService;
        private readonly ISignalRService _signalRService;
        private readonly IUploadFileService _uploadFileService;
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountService(IUnitOfWork unitOfWork, IMapper mapper,
            IConfiguration configuration, IHttpContextService httpContextService,
            ISendEmailService sendEmailService, INotificationService notificationService,
            ISignalRService signalRService, IUploadFileService uploadFileService,
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextService = httpContextService;
            _sendEmailService = sendEmailService;
            _notificationService = notificationService;
            _signalRService = signalRService;
            _uploadFileService = uploadFileService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AccountHomeResponse> GetDashboard()
        {
            var loggedModel = await _httpContextService.GetCurrentAccount();
            // Lấy danh sách đơn hàng của tài khoản hiện tại
            var orders = _unitOfWork.Repository<Transportation>()
                .GetQueryable()
                .Where(x => x.AccountId == loggedModel.Id);

            // Group theo trạng thái và đếm
            var counts = await orders
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Quantity = g.Count() })
                .ToListAsync();

            // Tạo list CountStatus cho tất cả trạng thái enum
            var countOrderList = Enum.GetValues(typeof(ETransportationStatus))
                .Cast<ETransportationStatus>()
                .Where(statusEnum => (int)statusEnum != (int)ETransportationStatus.Cancel)
                .Select(statusEnum =>
                {
                    var match = counts.FirstOrDefault(c => c.Status == (int)statusEnum);
                    return new CountStatus
                    {
                        Id = (int)statusEnum,
                        Quantity = match?.Quantity ?? 0
                    };
                })
                .OrderBy(x => x.Id)
                .ToList();

            var newNotifications = await _unitOfWork.Repository<Notification>().GetQueryable().Where(x =>
                        x.AccountId == loggedModel.Id
                         //&& x.IsRead == false 
                         && x.IsStaff == false
                    ).OrderByDescending(x => x.Id).Take(20).ToListAsync();

            var unPayOutOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().Where(x => x.AccountId == loggedModel.Id && x.StatusPayment != (int)EPaymentOutOfStockStatus.Paied).CountAsync();
            var voucher = await _unitOfWork.Repository<VoucherAccount>().GetQueryable().Where(x => x.AccountId == loggedModel.Id && x.Status == (int)EVoucherAccountStatus.New).CountAsync();
            return new AccountHomeResponse
            {
                Id = loggedModel.Id,
                Avatar = loggedModel.Avatar,
                Fullname = loggedModel.FullName,
                RoleName = ERoleIdName.GetRoleName(loggedModel.RoleId),
                Username = loggedModel.Username,
                CountOrder = countOrderList,
                NewNotifications = newNotifications,
                UnPayOutOfStock = unPayOutOfStock,
                Voucher = voucher,
            };
        }
        public async Task<bool> IsUserSession()
        {
            var loggedModel = await _httpContextService.GetCurrentAccount();
            if (loggedModel.RoleId != (int)ERoleId.User)
            {
                return false;
            }
            return true;
        }
        public async Task<Account> GetUserByUsernameChat(string username, string token)
        {
            var decryptAccount = AesEncryptionHelper.DecryptCurrentAccount(token);
            var currentAccount = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == decryptAccount.Id);
            if (currentAccount == null)
            {
                return new Account()
                {
                    Id = 0
                };
            }
            var query = _unitOfWork.Repository<Account>().GetQueryable();
            switch (currentAccount.RoleId)
            {
                case (int)ERoleId.User:
                    query = query.Where(x => x.RoleId != (int)ERoleId.Admin);
                    break;
                case (int)ERoleId.Sale:
                    query = query.Where(x => (x.RoleId == (int)ERoleId.User && x.SaleId == currentAccount.Id) || x.RoleId != (int)ERoleId.User);
                    break;
                default:
                    break;
            }
            var findUser = await query.SingleOrDefaultAsync(x => x.Username == username && x.Id != currentAccount.Id);
            if (findUser == null)
            {
                return new Account()
                {
                    Id = 0
                };
            }
            if (currentAccount.RoleId == (int)ERoleId.VNWarehouseStaff)
            {
                var sale = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == findUser.SaleId);
                if (sale != null)
                {
                    findUser.Username = $"{findUser.Username}<{sale.Username}>";
                }
            }
            return new Account
            {
                Id = findUser.Id,
                Username = findUser.Username,
                RoleId = findUser.RoleId
            };
        }

        public async Task<List<Account>> GetSales(int id)
        {
            var currentAccount = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            if (currentAccount == null)
            {
                return new List<Account>();
            }
            var sales = await _unitOfWork.Repository<Account>().GetQueryable()
                .Where(u => u.RoleId == (int)ERoleId.Sale && u.Id != currentAccount.Id
                && (currentAccount.SaleId == null || u.Id == currentAccount.SaleId)
                )
                .Select(u => new Account { Username = u.Username, Id = u.Id })
                .ToListAsync();
            return sales;
        }

        public async Task<AccountProfile> GetAccountProfile()
        {
            var loggedModel = await _httpContextService.GetCurrentAccount();
            var toWarehouses = await _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Destination && x.Status == (int)EWarehouseStatus.Active).ToListAsync();
            var toWarehouseId = toWarehouses.FirstOrDefault(x => x.Id == loggedModel.ToWarehouseId);
            return new AccountProfile
            {
                Username = loggedModel.Username,
                Address = loggedModel.Address,
                Avatar = loggedModel.Avatar,
                Email = loggedModel.Email,
                FullName = loggedModel.FullName,
                Phone = loggedModel.Phone,
                ToWarehouseId = toWarehouseId?.Id,
                Warehouses = toWarehouses,
            };

        }

        public async Task<Account> GetById(int id)
        {
            var currentLogged = _httpContextService.GetLoggedModel();
            var account = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            if (account == null)
            {
                throw new UnauthorizedAccessException();
            }
            if (currentLogged.RoleId == (int)ERoleId.Sale && currentLogged.Id != account.SaleId)
            {
                throw new UnauthorizedAccessException();
            }
            return account;
        }
        public async Task<List<AccountResponse>> GetIdAndUsernameByRole(int roleId)
        {
            return await _unitOfWork.Repository<Account>().GetQueryable()
                .Where(x => x.RoleId == roleId)
                .Select(x => new AccountResponse { Id = x.Id, Username = x.Username })
                .ToListAsync();
        }

        public async Task<List<AccountResponse>> getListAccount(AccountSearch search)
        {
            var query = (from account in _unitOfWork.Repository<Account>().GetQueryable()
                        join sale in _unitOfWork.Repository<Account>().GetQueryable().Where(x => x.RoleId == (int)ERoleId.Sale) on account.SaleId equals sale.Id into saleJoin
                        from sale in saleJoin.DefaultIfEmpty()
                        where (search.AccountId == null || account.Id == search.AccountId)
                            && (search.SaleId == null || account.SaleId == search.SaleId)
                            && (search.RoleId == null || account.RoleId == search.RoleId)
                            && (search.IsCustomer == null
                            || (search.IsCustomer == 1 && account.RoleId == (int)ERoleId.User)
                            || (search.IsCustomer == 0 && account.RoleId != (int)ERoleId.User))
                            && (search.PostOffice == null || account.PostOffice == search.PostOffice)
                            && (search.Username == null || account.Username.Contains(search.Username))
                        select new AccountResponse
                        {
                            Id = account.Id,
                            Username = account.Username,
                            FullName = account.FullName,
                            Email = account.Email,
                            Phone = account.Phone,
                            RoleId = account.RoleId,
                            SaleName = sale.Username,
                            Created = account.Created,
                        }).OrderByDescending(x => x.Id);
            var accounts = await query.ToListAsync();

            return accounts;
        }

        public async Task<PagedList<AccountResponse>> GetPaging(AccountSearch search)
        {
            var currentLogged = _httpContextService.GetLoggedModel();
            if (currentLogged.RoleId == (int)ERoleId.Sale)
            {
                search.SaleId = currentLogged.Id;
            }
            var accounts = await (from account in _unitOfWork.Repository<Account>().GetQueryable()
                                  join sale in _unitOfWork.Repository<Account>().GetQueryable().Where(x => x.RoleId == (int)ERoleId.Sale) on account.SaleId equals sale.Id into saleJoin
                                  from sale in saleJoin.DefaultIfEmpty()
                                  where (search.AccountId == null || account.Id == search.AccountId)
                                        && (search.SaleId == null || account.SaleId == search.SaleId)
                                        && (search.RoleId == null || account.RoleId == search.RoleId)
                                        && (search.IsCustomer == null
                                        || (search.IsCustomer == 1 && account.RoleId == (int)ERoleId.User)
                                        || (search.IsCustomer == 0 && account.RoleId != (int)ERoleId.User))
                                        && (search.PostOffice == null || account.PostOffice == search.PostOffice)
                                        && (search.Username == null || account.Username.Contains(search.Username))
                                  select new AccountResponse
                                  {
                                      Id = account.Id,
                                      Username = account.Username,
                                      FullName = account.FullName,
                                      Email = account.Email,
                                      Phone = account.Phone,
                                      RoleId = account.RoleId,
                                      SaleName = sale.Username,
                                      Created = account.Created,
                                  })
                            .OrderByDescending(x => x.Id)
                            .Skip((search.PageIndex - 1) * search.PageSize).Take(search.PageSize).ToListAsync();
            int total = await _unitOfWork.Repository<Account>().GetQueryable()
                .Where(x => (search.AccountId == null || x.Id == search.AccountId)
                                && (search.SaleId == null || x.SaleId == search.SaleId)
                                && (search.RoleId == null || x.RoleId == search.RoleId)
                                && (search.IsCustomer == null
                                    || (search.IsCustomer == 1 && x.RoleId == (int)ERoleId.User)
                                    || (search.IsCustomer == 0 && x.RoleId != (int)ERoleId.User))
                                && (search.PostOffice == null || x.PostOffice == search.PostOffice)
                                && (search.Username == null || x.Username.Contains(search.Username))
                                )
                .CountAsync();
            var pricingSeparates = await GetPricingSeparate(accounts.Select(a => a.Id).ToList());
            foreach (var account in accounts)
            {
                account.PricingResponses = pricingSeparates.Where(x => x.AccountId == account.Id).ToList();
            }

            foreach (var account in accounts)
            {
                var salesAmount = await (
                    from w in _unitOfWork.Repository<Warehouse>().GetQueryable()
                                .Where(x => x.Type == (int)EWarehouseType.Shipping)

                    join t in _unitOfWork.Repository<Transportation>().GetQueryable()
                                .Where(x => x.AccountId == account.Id && x.Status == (int)ETransportationStatus.Completed)
                    on w.Id equals t.ShipId into gw

                    from t in gw.DefaultIfEmpty()

                    group t by new { w.Id, w.Name } into g

                    select new AccountSalesResponse
                    {
                        Id = g.Key.Id,
                        ShipName = g.Key.Name,
                        Amount = g.Sum(x => x != null ? x.TotalPriceVND : 0)
                    }
                ).OrderBy(x => x.Id).ToListAsync();

                account.AccountSalesResponses = salesAmount;
            }

            return new PagedList<AccountResponse>
            {
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total,
                Items = accounts
            };
        }

        public async Task<Account> SigninCookieAsync(string username, string password)
        {
            Log.Information($"Sign Up: {username}; {password};");
            var account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Username == username);

            if (account == null || account?.PasswordHash != EncryptPassword(password.Insert(2, account?.PasswordEncryptValue), password))
            {
                throw new AppException("Thông tin đăng nhập không đúng");
            }
            return account;
        }

        public async Task<AuthenticationResponse> SignupAsync(CreateAccountRequest request)
        {
            var isValid = Regex.IsMatch(request.Username, @"^[a-zA-Z0-9]+$");

            if (!isValid)
            {
                throw new AppException("Username không được chứa dấu hoặc khoảng trắng");
            }
            Log.Information($"Sign Up: {request.Username}; {request.Password};");
            var account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Username == request.Username);
            if (account != null)
            {
                throw new AppException("Username đã tồn tại");
            }
            account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Email == request.Email);
            if (account != null)
            {
                throw new AppException("Email đã tồn tại");
            }
            account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Phone == request.Phone);
            if (account != null)
            {
                throw new AppException("Số điện thoại đã tồn tại");
            }
            string passwordEncryptValue = RandomStringWithText(16);
            string passwordEncryptKey = request.Password.Insert(2, passwordEncryptValue);
            var passwordHash = EncryptPassword(passwordEncryptKey, request.Password);
            var newAccount = _mapper.Map<Account>(request);
            newAccount.PasswordHash = passwordHash;
            newAccount.RoleId = (int)ERoleId.User;
            newAccount.PasswordEncryptValue = passwordEncryptValue;
            newAccount.PostOffice = PostOfficeName.GetPostOffice().FirstOrDefault();

            try
            {
                await _unitOfWork.Repository<Account>().Add(newAccount, DateTime.Now, 0);
                await _unitOfWork.SaveAsync();
                var notification = new Notification
                {
                    Title = "Khách hàng mới",
                    Content = $"Khách hàng mới đăng ký Username: {newAccount.Username}",
                    WebUrl = "/customer",
                    Type = (int)ENotificationType.NewCustomer,
                    IsStaff = true,
                };
                await _notificationService.SendNotification(notification, DateTime.Now, 0, staffRoleIds: new List<int> { (int)ERoleId.Admin, (int)ERoleId.Manager, });
                string accessToken = GenerateJwtToken(newAccount);
                return new AuthenticationResponse()
                {
                    Token = accessToken,
                };
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }

        public async Task<bool> SendEmailPassword(string email)
        {
            var account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Email == email.Trim());
            if (account == null)
            {
                throw new AppException("Không tìm thấy tài khoản");
            }
            var newPassword = GenerateRandomPassword();
            string passwordEncryptValue = RandomStringWithText(16);
            string passwordEncryptKey = newPassword.Insert(2, passwordEncryptValue);
            var passwordHash = EncryptPassword(passwordEncryptKey, newPassword);
            account.PasswordHash = passwordHash;
            account.PasswordEncryptValue = passwordEncryptValue;
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.Repository<Account>().Update(account, DateTime.Now, 0);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                _sendEmailService.Send(email, "Mật khẩu mới", $"Mật khẩu tài khoản {account.Username}: {newPassword}");
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }

        private string GenerateJwtToken(Account account)
        {
            var claims = new List<Claim>
            {
                new Claim("Username", account.Username),
                new Claim("Id", account.Id.ToString()),
                new Claim("Role", account.RoleId.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> Update(int id, UpdateAccountRequest request)
        {
            var account = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var newSaleName = "";

            var loggedModel = await _httpContextService.GetCurrentAccount();
            if (loggedModel.RoleId == (int)ERoleId.Sale)
            {
                request.SaleId = loggedModel.Id;
            }
            if (request.SaleId != account.SaleId)
            {
                var newSale = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.SaleId);
                if (newSale != null)
                {
                    newSaleName = newSale.Username;
                    var vnWarehouseStaffs = await _unitOfWork.Repository<Account>().GetQueryable().Where(x => x.RoleId == (int)ERoleId.VNWarehouseStaff).Select(x => x.Id).ToListAsync();
                    if (vnWarehouseStaffs != null && vnWarehouseStaffs.Any())
                    {
                        string query = $@"
                        UPDATE ChatConversations
                        SET Name = '{account.Username}<{newSale.Username}>' + 
                            CASE 
                                WHEN CHARINDEX(',', Name) > 0 THEN
                                    SUBSTRING(Name, CHARINDEX(',', Name), LEN(Name)) 
                                ELSE
                                    ''
                            END
                        WHERE CustomerId = {account.Id} AND StaffId IN ({string.Join(',', vnWarehouseStaffs)})";
                        var httpClient = _httpClientFactory.CreateClient();

                        var content = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");

                        var response = await httpClient.PostAsync("https://chat.tpkexpress.cn/conversion", content);
                    }
                }
            }
            if (request.NewPassword != null)
            {
                string passwordEncryptValue = RandomStringWithText(16);
                string passwordEncryptKey = request.NewPassword.Insert(2, passwordEncryptValue);
                var passwordHash = EncryptPassword(passwordEncryptKey, request.NewPassword);
                account.PasswordHash = passwordHash;
                account.PasswordEncryptValue = passwordEncryptValue;
            }
            _mapper.Map(request, account);
            if (!(account.RoleId > 0))
            {
                account.RoleId = (int)ERoleId.User;
            }
            _unitOfWork.Repository<Account>().Update(account, currentDate, currentAccount.Id);

            if (request.VNWarehouseStaffIds != null)
            {
                var currentSupervisors = await _unitOfWork.Repository<AccountWarehouseSupervisor>()
                    .GetQueryable()
                    .Where(x => x.SaleId == account.Id)
                    .Select(x => x.WarehouseStaffId)
                    .ToListAsync();

                var newSupervisorIds = request.VNWarehouseStaffIds;

                var toAdd = newSupervisorIds.Except(currentSupervisors).ToList();
                var toRemove = currentSupervisors.Except(newSupervisorIds).ToList();

                foreach (var idToAdd in toAdd)
                {
                    var newItem = new AccountWarehouseSupervisor
                    {
                        SaleId = account.Id,
                        WarehouseStaffId = idToAdd
                    };
                    await _unitOfWork.Repository<AccountWarehouseSupervisor>().Add(newItem, currentDate, currentAccount.Id);
                }

                if (toRemove.Any())
                {
                    var removeList = await _unitOfWork.Repository<AccountWarehouseSupervisor>()
                        .GetQueryable()
                        .Where(x => x.SaleId == account.Id && toRemove.Contains(x.WarehouseStaffId))
                        .ToListAsync();

                    foreach (var item in removeList)
                    {
                        _unitOfWork.Repository<AccountWarehouseSupervisor>().Delete(item);
                    }
                }
            }

            await _unitOfWork.SaveAsync();
            await _signalRService.SendRemoteLogoutToUser(account.Id);
            if (newSaleName != "")
            {
                var notification = new Notification
                {
                    Title = "Gán khách hàng",
                    Content = $"{currentAccount.Username} đã gán khách {account.Username} cho nhân viên {newSaleName}",
                    WebUrl = $"/account-detail/{id}",
                    Type = (int)ENotificationType.NewCustomer,
                    IsStaff = true,
                };
                await _notificationService.SendNotification(notification, currentDate, currentAccount.Id,
                    staffId: request.SaleId ?? 0, staffRoleIds: new List<int> { (int)ERoleId.Admin, (int)ERoleId.Manager, });
            }
            return true;
        }
        public async Task<bool> UpdateProfile(AccountProfile request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();

            if (request.Password != request.ConfirmPassword)
            {
                throw new AppException("Nhập lại mật khẩu không trùng với mật khẩu");
            }
            var checkEmail = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Email == request.Email);
            if (checkEmail != null && checkEmail?.Id != currentAccount.Id)
            {
                throw new AppException("Email đã được sử dụng");
            }
            var checkPhone = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Phone == request.Phone);
            if (checkPhone != null && checkPhone?.Id != currentAccount.Id)
            {
                throw new AppException("Email đã được sử dụng");
            }
            if (request.Password != null)
            {
                string passwordEncryptValue = RandomStringWithText(16);
                string passwordEncryptKey = request.Password.Insert(2, passwordEncryptValue);
                var passwordHash = EncryptPassword(passwordEncryptKey, request.Password);
                currentAccount.PasswordHash = passwordHash;
                currentAccount.PasswordEncryptValue = passwordEncryptValue;
            }
            if (request.FileNewAvatar != null)
            {
                currentAccount.Avatar = await _uploadFileService.UploadImage(request.FileNewAvatar);
            }
            currentAccount.Email = request.Email;
            currentAccount.Phone = request.Phone;
            currentAccount.Address = request.Address;
            currentAccount.FullName = request.FullName;
            currentAccount.ToWarehouseId = request.ToWarehouseId;
            _unitOfWork.Repository<Account>().Update(currentAccount, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> Create(CreateAccountRequest request)
        {
            var isValid = Regex.IsMatch(request.Username, @"^[a-zA-Z0-9]+$");

            if (!isValid)
            {
                throw new AppException("Username không được chứa dấu hoặc khoảng trắng");
            }
            var account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Username == request.Username);
            if (account != null)
            {
                throw new AppException("Username đã tồn tại");
            }
            account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Email == request.Email);
            if (account != null)
            {
                throw new AppException("Email đã tồn tại");
            }
            var currentLogged = _httpContextService.GetLoggedModel();
            if (currentLogged.RoleId == (int)ERoleId.Sale)
            {
                request.SaleId = currentLogged.Id;
            }
            string passwordEncryptValue = RandomStringWithText(16);
            string passwordEncryptKey = request.Password.Insert(2, passwordEncryptValue);
            var passwordHash = EncryptPassword(passwordEncryptKey, request.Password);
            var newAccount = _mapper.Map<Account>(request);
            newAccount.PasswordHash = passwordHash;
            newAccount.PasswordEncryptValue = passwordEncryptValue;
            newAccount.RoleId = (int)ERoleId.User;
            newAccount.PostOffice = PostOfficeName.GetPostOffice().FirstOrDefault();
            await _unitOfWork.Repository<Account>().Add(newAccount, DateTime.Now, currentLogged.Id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<List<PricingResponse>> GetPricingSeparate(List<int> accountIds)
        {
            return await (from pricing in _unitOfWork.Repository<PricingSeparate>().GetQueryable()
                          join shippingType in _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Shipping) on pricing.ShipId equals shippingType.Id into shippingTypeJoin
                          from shippingType in shippingTypeJoin.DefaultIfEmpty()
                          where accountIds.Contains(pricing.AccountId)
                          select new PricingResponse
                          {
                              Id = pricing.Id,
                              RangeMin = pricing.RangeMin,
                              RangeMax = pricing.RangeMax,
                              PricePerUnit = pricing.PricePerUnit,
                              Type = pricing.Type,
                              ShipId = pricing.ShipId,
                              ShipName = shippingType.Name,
                              Created = pricing.Created,
                              CreatedBy = pricing.CreatedBy,
                              Updated = pricing.Updated,
                              UpdateBy = pricing.UpdateBy,
                              AccountId = pricing.AccountId,
                          })
                     .OrderBy(x => x.ShipId)
                     .ThenBy(x => x.RangeMin)
                     .ToListAsync();
        }

        public async Task<bool> CreatePricingSeparate(int accountId, CreatePricingSeparateRequest request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var customerAccount = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == accountId);
            if (customerAccount == null)
            {
                throw new AppException("Không tìm thấy tài khoản");
            }
            var listPricingSeparate = new List<PricingSeparate>();
            if ((request.RangeWeightMax + request.RangeWeightMax + request.PricePerWeightUnit) > 0)
            {
                listPricingSeparate.Add(new PricingSeparate
                {
                    AccountId = accountId,
                    ShipId = request.ShipId,
                    Type = (int)EPricingType.Weight,
                    RangeMin = request.RangeWeightMin ?? 0,
                    RangeMax = request.RangeWeightMax ?? 0,
                    PricePerUnit = (decimal)(request.PricePerWeightUnit ?? 0),
                });
            }
            if ((request.RangeVolumeMax + request.RangeVolumeMax + request.PricePerVolumeUnit) > 0)
            {
                listPricingSeparate.Add(new PricingSeparate
                {
                    AccountId = accountId,
                    ShipId = request.ShipId,
                    Type = (int)EPricingType.Volume,
                    RangeMin = request.RangeVolumeMin ?? 0,
                    RangeMax = request.RangeVolumeMax ?? 0,
                    PricePerUnit = (decimal)(request.PricePerVolumeUnit ?? 0),
                });
            }
            if (listPricingSeparate.Count > 0)
            {
                await _unitOfWork.Repository<PricingSeparate>().AddRange(listPricingSeparate, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();
            }
            return true;
        }

        public async Task<bool> UpdatePricingSeparate(int accountId, PricingSeparate request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var pricingSeparate = await _unitOfWork.Repository<PricingSeparate>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.Id && x.AccountId == accountId);
            if (pricingSeparate == null)
            {
                throw new AppException("Không tìm thấy bảng giá");
            }
            pricingSeparate.RangeMin = request.RangeMin;
            pricingSeparate.RangeMax = request.RangeMax;
            pricingSeparate.PricePerUnit = request.PricePerUnit;
            pricingSeparate.ShipId = request.ShipId;
            pricingSeparate.Type = request.Type;
            _unitOfWork.Repository<PricingSeparate>().Update(pricingSeparate, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> DeletePricingSeparate(int accountId, int id)
        {
            var pricingSeparate = await _unitOfWork.Repository<PricingSeparate>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id && x.AccountId == accountId);
            _unitOfWork.Repository<PricingSeparate>().Delete(pricingSeparate);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<List<AccountWarehouseSupervisor>> GetAccountWarehouseSupervisorsBySaleId(int id)
        {
            return await _unitOfWork.Repository<AccountWarehouseSupervisor>().GetQueryable().Where(x => x.SaleId == id).ToListAsync();
        }
        private string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string RandomStringWithText(int numberrandom)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[numberrandom];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }

        private string EncryptPassword(string key, string data)
        {
            data = data.Trim();
            byte[] keydata = Encoding.ASCII.GetBytes(key);
            string md5String = BitConverter.ToString(new
            MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-", "").ToLower();
            byte[] tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));
            TripleDES tripdes = TripleDESCryptoServiceProvider.Create();
            tripdes.Mode = CipherMode.ECB;
            tripdes.Key = tripleDesKey;
            tripdes.GenerateIV();
            MemoryStream ms = new MemoryStream();
            CryptoStream encStream = new CryptoStream(ms, tripdes.CreateEncryptor(),
            CryptoStreamMode.Write);
            encStream.Write(Encoding.ASCII.GetBytes(data), 0,
            Encoding.ASCII.GetByteCount(data));
            encStream.FlushFinalBlock();
            byte[] cryptoByte = ms.ToArray();
            ms.Close();
            encStream.Close();
            return Convert.ToBase64String(cryptoByte, 0, cryptoByte.GetLength(0)).Trim();
        }


    }
}
