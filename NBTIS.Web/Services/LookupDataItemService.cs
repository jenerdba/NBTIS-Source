using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NBTIS.Core.Hub;
using NBTIS.Data.Models;
using System.Net.Http;
using Telerik.DataSource.Extensions;
using System.Data;
using ErrorSummary = NBTIS.Web.ViewModels.ErrorSummary;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;

namespace NBTIS.Web.Services
{
    public class LookupDataItemService
    {

        private readonly ILogger<LookupDataItemService> _logger;
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<DataContext> _contextFactory;

        public LookupDataItemService(
            ILogger<LookupDataItemService> logger,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IDbContextFactory<DataContext> contextFactory,
            IMapper mapper
            )
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _mapper = mapper;
        }

        public async Task<Lookup_DataItem?> GetDataItemAsync(string itemID)
        {
            using var context = _contextFactory.CreateDbContext();

            var dataItem = await context.Lookup_DataItems
                    .Where(item => item.ItemId == itemID)
                    .FirstOrDefaultAsync();

                if (dataItem == null)
                    return null;

                return new Lookup_DataItem
                {
                    ItemId = dataItem.ItemId,
                    ItemName = dataItem.ItemName,
                    Identifier = dataItem.Identifier,
                    NBI_Id = dataItem.NBI_Id
                };

        }
    }

}
