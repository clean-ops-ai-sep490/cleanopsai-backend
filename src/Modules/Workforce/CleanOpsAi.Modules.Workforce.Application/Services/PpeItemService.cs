using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.PpeItems;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using Medo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Services
{
    public class PpeItemService : IPpeItemService
    {
        private readonly IPpeItemRepository _ppeRepository;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private const string CONTAINER = "contracts";
        private const string IMAGE_FOLDER = "ppes";
        private readonly IFileStorageService _fileStorage;

        public PpeItemService(IPpeItemRepository ppeRepository, IUserContext userContext, IDateTimeProvider dateTimeProvider, IFileStorageService fileStorage)
        {
            _ppeRepository = ppeRepository;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _fileStorage = fileStorage;
        }

        // get by id
        public async Task<List<PpeItemResponse>?> GetByIdAsync(Guid id)
        {
            var item = await _ppeRepository.GetByIdAsync(id);

            if (item == null)
                return null;

            return new List<PpeItemResponse>
            {
                new PpeItemResponse
                {
                    Id = item.Id,
                    ActionKey = item.ActionKey,
                    Name = item.Name,
                    Description = item.Description,
                    ImageUrl = item.ImageUrl
                }
            };
        }

        // get all
        public async Task<List<PpeItemResponse>> GetAllAsync()
        {
            var items = await _ppeRepository.GetAllAsync();

            return items.Select(x => new PpeItemResponse
            {
                Id = x.Id,
                ActionKey = x.ActionKey,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl
            }).ToList();
        }

        // pagination
        public async Task<PagedResponse<PpeItemResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _ppeRepository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new PpeItemResponse
            {
                Id = x.Id,
                ActionKey = x.ActionKey,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl
            }).ToList();

            return new PagedResponse<PpeItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // create
        public async Task<PpeItemResponse> CreateAsync(PpeItemCreateRequest request)
        {
            string imageUrl = "";

            //  upload giống Worker
            if (request.ImageStream != null && !string.IsNullOrWhiteSpace(request.ImageFileName))
            {
                imageUrl = await _fileStorage.UploadFileAsync(
                    request.ImageStream,
                    $"{IMAGE_FOLDER}/{request.ImageFileName}",
                    CONTAINER
                );
            }

            var entity = new PpeItem
            {
                Id = Uuid7.NewGuid(),
                ActionKey = request.ActionKey.Trim().ToLower(),
                Name = request.Name,
                Description = request.Description,
                ImageUrl = imageUrl,
                Created = _dateTimeProvider.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),
                IsDeleted = false
            };

            await _ppeRepository.CreateAsync(entity);

            return new PpeItemResponse
            {
                Id = entity.Id,
                ActionKey = entity.ActionKey,
                Name = entity.Name,
                Description = entity.Description,
                ImageUrl = entity.ImageUrl
            };
        }

        // update
        public async Task<PpeItemResponse> UpdateAsync(Guid id, PpeItemUpdateRequest request)
        {
            var entity = await _ppeRepository.GetByIdAsync(id);

            if (entity == null)
                throw new NotFoundException($"PpeItem with id {id} not found.");

            entity.ActionKey = string.IsNullOrWhiteSpace(request.ActionKey)
                ? entity.ActionKey
                : request.ActionKey.Trim().ToLower();

            entity.Name = string.IsNullOrWhiteSpace(request.Name)
                ? entity.Name
                : request.Name;

            entity.Description = string.IsNullOrWhiteSpace(request.Description)
                ? entity.Description
                : request.Description;

            if (request.ImageStream != null && !string.IsNullOrWhiteSpace(request.ImageFileName))
            {
                var fileUrl = await _fileStorage.UploadFileAsync(
                    request.ImageStream,
                    $"{IMAGE_FOLDER}/{request.ImageFileName}",
                    CONTAINER
                );

                entity.ImageUrl = fileUrl;
            }

            entity.LastModified = _dateTimeProvider.UtcNow;
            entity.LastModifiedBy = _userContext.UserId.ToString();

            await _ppeRepository.UpdateAsync(entity);

            return new PpeItemResponse
            {
                Id = entity.Id,
                ActionKey = entity.ActionKey,
                Name = entity.Name,
                Description = entity.Description,
                ImageUrl = entity.ImageUrl
            };
        }

        // delete
        public async Task<int> DeleteAsync(Guid id)
        {
            var entity = await _ppeRepository.GetByIdAsync(id);

            if (entity == null)
                throw new NotFoundException($"PpeItem with id {id} not found.");

            return await _ppeRepository.DeleteAsync(id);
        }

        // filter theo actionKey
        public async Task<List<PpeItemResponse>> GetByActionKeyAsync(string actionKey)
        {
            if (string.IsNullOrWhiteSpace(actionKey))
                return new List<PpeItemResponse>();

            var items = await _ppeRepository.GetByActionKeyAsync(actionKey.Trim().ToLower());

            return items.Select(x => new PpeItemResponse
            {
                Id = x.Id,
                ActionKey = x.ActionKey,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl
            }).ToList();
        }

        // get distinct actionKey
        public async Task<List<string>> GetAllActionKeysAsync()
        {
            return await _ppeRepository.GetAllActionKeysAsync();
        }
    }
}
