using AutoMapper;
using Azure.Core;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;
using static WebMVC.Services.ZaloAPIService;
namespace WebMVC.Services
{
    public class ContactService : IContactService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextService _httpContextService;
        private readonly ISignalRService _signalRService;
        private readonly INotificationService _notificationService;
        private readonly IZaloAPIService _zaloAPIService;
        private readonly IUploadFileService _uploadFileService;

        public ContactService(IUnitOfWork unitOfWork, IMapper mapper,
            IHttpContextService httpContextService, ISignalRService signalRService,
            INotificationService notificationService, IZaloAPIService zaloAPIService,
            IUploadFileService uploadFileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextService = httpContextService;
            _signalRService = signalRService;
            _notificationService = notificationService;
            _zaloAPIService = zaloAPIService;
            _uploadFileService = uploadFileService;
        }

        public async Task<PagedList<ContactListResponse>> GetPaging(ContactSearch search)
        {
            var loggedModel = _httpContextService.GetLoggedModel();
            return await GetPagingForAPI(search, loggedModel.Id);

        }

        public async Task<ResponseClass> GetPagingApi(ContactListApiSearch body)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(body.user.Key, (int) body.user.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            rs.data = await GetPagingForAPI(body.contactSearch, currentAccount.Id);
            return rs;
        }

        public async Task<PagedList<ContactListResponse>> GetPagingForAPI(ContactSearch search, int accountId)
        {

            var query = _unitOfWork.Repository<ContactList>().GetQueryable();
            if (!string.IsNullOrWhiteSpace(search.Name))
            {
                query = query.Where(x => x.Name.Contains(search.Name));
            }

            query = query.Where(x => x.CreatedBy == accountId);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Created)
                .Skip((search.PageIndex - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(x => new ContactListResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Created = x.Created,
                    Contacts = x.ContactListContacts
                        .Select(clc => new Contact
                        {
                            Id = clc.Contact.Id,
                            FirstName = clc.Contact.FirstName,
                            LastName = clc.Contact.LastName,
                            Email = clc.Contact.Email
                        })
                        .ToList()
                })
                .ToListAsync();
            // Console.WriteLine(
            //     "items"
            // );
            // Console.WriteLine(
            //     JsonConvert.SerializeObject(items, Formatting.Indented)
            // );
            // var responseItems = items.Select(x => new CampaignResponse
            // {
            //     Id = x.Id,
            //     Name = x.Name,
            //     Subject = x.Subject,
            //     Status = x.Status,
            //     Body = x.Body,
            //     Created = x.Created,
            //     CreatedBy = x.CreatedBy,
            //     Updated = x.Updated,
            //     UpdateBy = x.UpdateBy
            // }).ToList();

            return new PagedList<ContactListResponse>
            {
                Items = items,
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total
            };
        }
        public async Task<ContactListResponse> GetContactById(int id, string email = null)
        {
            var template = await _unitOfWork.Repository<ContactList>().GetQueryable().Select(x => new ContactListResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Contacts = x.ContactListContacts
                        .Where(clc => string.IsNullOrWhiteSpace(email) || clc.Contact.Email.Contains(email))
                        .Select(clc => new Contact
                        {
                            Id = clc.Contact.Id,
                            FirstName = clc.Contact.FirstName,
                            LastName = clc.Contact.LastName,
                            Email = clc.Contact.Email
                        })
                        .ToList()
                }).FirstOrDefaultAsync(x => x.Id == id);
            return template;
        }

        public async Task<ResponseClass> GetContactList(int id, AppUser body)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(body.Key, (int) body.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var list = await _unitOfWork.Repository<ContactList>().GetQueryable().Select(x => new ContactListResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Contacts = x.ContactListContacts
                        .Select(clc => new Contact
                        {
                            Id = clc.Contact.Id,
                            FirstName = clc.Contact.FirstName,
                            LastName = clc.Contact.LastName,
                            Email = clc.Contact.Email
                        })
                        .ToList()
                }).FirstOrDefaultAsync(x => x.Id == id);
            rs.data = list;
            return rs;
        }

        public async Task<ResponseClass> GetEmailContactList(int id, EmailContactListSearch body)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(body.user.Key, (int) body.user.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var query = _unitOfWork.PlainRepository<ContactListContact>().GetQueryable()
                .Where(clc => clc.ContactListId == id);

            if (!string.IsNullOrWhiteSpace(body.Email))
            {
                query = query.Where(clc => clc.Contact.Email.Contains(body.Email));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(clc => clc.Contact.Id)
                .Skip((body.PageIndex - 1) * body.PageSize)
                .Take(body.PageSize)
                .Select(clc => new Contact
                {
                    Id = clc.Contact.Id,
                    FirstName = clc.Contact.FirstName,
                    LastName = clc.Contact.LastName,
                    Email = clc.Contact.Email
                })
                .ToListAsync();

            rs.data = new PagedList<Contact>
            {
                Items = items,
                PageIndex = body.PageIndex,
                PageSize = body.PageSize,
                TotalItem = total
            };
            return rs;
        }

        public async Task CreateList(CreateContactListRequest request,Account currentAccount)
        {
            var currentDate = DateTime.Now;
            var contactList = new ContactList
            {
                Name = request.Name,
            };

            await _unitOfWork.Repository<ContactList>().Add(contactList, currentDate, currentAccount.Id);
            if (request.File != null && request.File.Length > 0)
            {
                using var stream = new MemoryStream();
                await request.File.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);

                // Bỏ qua dòng header (dòng 1)
                var rows = worksheet.RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    var firstName = row.Cell(1).GetString().Trim();
                    var lastName = row.Cell(2).GetString().Trim();
                    var email = row.Cell(3).GetString().Trim();


                    if (string.IsNullOrWhiteSpace(firstName) &&
                        string.IsNullOrWhiteSpace(lastName) &&
                        string.IsNullOrWhiteSpace(email))
                    {
                        continue;
                    }

                    var contact = await _unitOfWork.Repository<Contact>().GetQueryable()
                        .FirstOrDefaultAsync(x => x.Email == email);

                    if (contact == null)
                    {
                        contact = new Contact
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            Email = email,
                        };
                        await _unitOfWork.Repository<Contact>().Add(contact, currentDate, currentAccount.Id);
                    }
                    else
                    {
                        contact.FirstName = firstName;
                        contact.LastName = lastName;
                        _unitOfWork.Repository<Contact>().Update(contact, currentDate, currentAccount.Id);
                    }

                    var contactListContact = new ContactListContact
                    {
                        ContactList = contactList,
                        Contact = contact,
                    };

                    await _unitOfWork.PlainRepository<ContactListContact>().AddAsync(contactListContact);
                }
            }

            await _unitOfWork.SaveAsync();
        }
        public async Task CreateAsync(CreateContactListRequest request)
        {
            var currentAccount = await _httpContextService.GetCurrentAccount();
            await CreateList(request, currentAccount);
        }
        public async Task<ResponseClass> CreateApiAsync(CreateContactListApiRequest body)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(body.Key, (int) body.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            await CreateList(body.request, currentAccount);
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            return rs;
        }
        public async Task SaveAsync(int id, AddContactListRequest request)
        {
            var contactListExists = await _unitOfWork.Repository<ContactList>()
            .GetQueryable()
            .AnyAsync(x => x.Id == id);

            if (!contactListExists)
            {
                throw new Exception("ContactList not found");
            }
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var contact = new ContactList
            {
                Id = id,
                Name = request.Name,
            };

            _unitOfWork.Repository<ContactList>().Update(contact, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
        }

        public async Task<ResponseClass> UpdateContactList(int id, CreateContactListApiRequest body)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(body.Key, (int) body.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var request = body.request;
            var contactListExists = await _unitOfWork.Repository<ContactList>()
            .GetQueryable()
            .AnyAsync(x => x.Id == id);

            if (!contactListExists)
            {
                throw new Exception("ContactList not found");
            }
            var currentDate = DateTime.Now;
            var contact = new ContactList
            {
                Id = id,
                Name = request.Name,
            };

            _unitOfWork.Repository<ContactList>().Update(contact, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            rs.data = contact;
            return rs;
        }

        public async Task<Contact> addContact(AddContactRequest request)
        {
            
            var contactListExists = await _unitOfWork.Repository<ContactList>()
            .GetQueryable()
            .AnyAsync(x => x.Id == request.Id);

            if (!contactListExists)
            {
                throw new Exception("ContactList not found");
            }
            var emailExists = await _unitOfWork.PlainRepository<ContactListContact>()
                .GetQueryable()
                .AnyAsync(clc => clc.ContactListId == request.Id && clc.Contact.Email == request.Email);

            if (emailExists)
            {
                throw new Exception("Email already exists in this contact list");
            }
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var contact = new Contact
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
            };

            await _unitOfWork.Repository<Contact>().Add(contact, currentDate, currentAccount.Id);
            var contactListContact = new ContactListContact
            {
                ContactListId = request.Id,
                Contact = contact,
            };
            await _unitOfWork.PlainRepository<ContactListContact>().AddAsync(contactListContact);

            await _unitOfWork.SaveAsync();
            return contact;
        }

        public async Task<ResponseClass> AddEmailContactList(int id, AddContactRequestApi body)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(body.Key, body.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var request = body.request;
            var contactListExists = await _unitOfWork.Repository<ContactList>()
            .GetQueryable()
            .AnyAsync(x => x.Id == request.Id);

            if (!contactListExists)
            {
                throw new Exception("ContactList not found");
            }
            var emailExists = await _unitOfWork.PlainRepository<ContactListContact>()
                .GetQueryable()
                .AnyAsync(clc => clc.ContactListId == request.Id && clc.Contact.Email == request.Email);

            if (emailExists)
            {
                throw new Exception("Email already exists in this contact list");
            }
            var currentDate = DateTime.Now;
            var contact = new Contact
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
            };

            await _unitOfWork.Repository<Contact>().Add(contact, currentDate, currentAccount.Id);
            var contactListContact = new ContactListContact
            {
                ContactListId = request.Id,
                Contact = contact,
            };
            await _unitOfWork.PlainRepository<ContactListContact>().AddAsync(contactListContact);

            await _unitOfWork.SaveAsync();
            rs.data = contact;
            return rs;
        }

        public async Task<List<ContactList>> GetAll()
        {
            var query = _unitOfWork.Repository<ContactList>().GetQueryable();
            var items = await query
                .AsNoTracking()
                .OrderByDescending(x => x.Created)
                .Select(x => new ContactList
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
            return items;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<ContactList>().GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                throw new Exception("Không tìm thấy dữ liệu");
            }

            _unitOfWork.Repository<ContactList>().Delete(entity);

            await _unitOfWork.SaveAsync();
        }
        public async Task<ResponseClass> DeleteApiAsync(int id, AppUser appUser)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(appUser.Key, appUser.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var entity = await _unitOfWork.Repository<ContactList>().GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                throw new Exception("Không tìm thấy dữ liệu");
            }

            _unitOfWork.Repository<ContactList>().Delete(entity);

            await _unitOfWork.SaveAsync();
            return rs;
        }
        
        public async Task EditContactAsync(UpdateContactRequest request)
        {
            var entity = await _unitOfWork.Repository<Contact>().GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (entity == null)
                throw new Exception("Không tìm thấy contact");

            var emailTaken = await _unitOfWork.Repository<Contact>().GetQueryable()
                .AnyAsync(x => x.Email == request.Email && x.Id != request.Id);

            if (emailTaken)
                throw new Exception("Email đã được sử dụng");

            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();

            entity.FirstName = request.FirstName;
            entity.LastName = request.LastName;
            entity.Email = request.Email;

            _unitOfWork.Repository<Contact>().Update(entity, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
        }

        public async Task<ResponseClass> UpdateContactApi(int id, UpdateContactRequestApi body)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(body.Key, body.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var request = body.request;
            var entity = await _unitOfWork.Repository<Contact>().GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (entity == null)
                throw new Exception("Không tìm thấy contact");

            var emailTaken = await _unitOfWork.Repository<Contact>().GetQueryable()
                .AnyAsync(x => x.Email == request.Email && x.Id != request.Id);

            if (emailTaken)
                throw new Exception("Email đã được sử dụng");

            var currentDate = DateTime.Now;

            entity.FirstName = request.FirstName;
            entity.LastName = request.LastName;
            entity.Email = request.Email;

            _unitOfWork.Repository<Contact>().Update(entity, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            rs.data = entity;
            return rs;
        }

        public async Task DeleteContact(int id, int ContactListId)
        {
            var entity = await _unitOfWork.PlainRepository<ContactListContact>().GetQueryable()
                .FirstOrDefaultAsync(x => x.ContactId == id && x.ContactListId == ContactListId);

            if (entity == null)
            {
                throw new Exception("Không tìm thấy dữ liệu");
            }

            _unitOfWork.PlainRepository<ContactListContact>().Remove(entity);

            await _unitOfWork.SaveAsync();
        }
        public async Task<ResponseClass> DeleteContactApi(int id, int ContactListId, AppUser appUser)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(appUser.Key, appUser.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var entity = await _unitOfWork.PlainRepository<ContactListContact>().GetQueryable()
                .FirstOrDefaultAsync(x => x.ContactId == id && x.ContactListId == ContactListId);

            if (entity == null)
            {
                throw new Exception("Không tìm thấy dữ liệu");
            }

            _unitOfWork.PlainRepository<ContactListContact>().Remove(entity);

            await _unitOfWork.SaveAsync();
            return rs;
        }
    }
}
