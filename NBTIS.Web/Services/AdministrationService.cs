using AutoMapper;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NBTIS.Core.Hub;
using NBTIS.Data.Models;
using Telerik.DataSource.Extensions;
using static NBTIS.Web.Services.SubmittalService;
using System.Data;


namespace NBTIS.Web.Services
{
    public class AdministrationService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<AdministrationService> _logger;
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public AdministrationService(
            IHttpContextAccessor httpContextAccessor,
            HttpClientService httpClientService,
            IHubContext<MessageHub> hubContext,
            ILogger<AdministrationService> logger,
            IConfiguration configuration,
            IWebHostEnvironment env,
            DataContext dataContext,
            IMapper mapper

            )
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientService.Client;
            _hubContext = hubContext;
            _logger = logger;
            _context = dataContext;
            _mapper = mapper;

        }

        public async Task<bool> UpdateSubmittalErrorsIgnoreAsync()
        {
            var errorRecords = await _context.SubmittalErrors.Where(s => s.Ignore == true).ToListAsync();

            if (errorRecords.Any())
            {
                // Update Ignore to False for each record
                foreach (var record in errorRecords)
                {
                    record.Ignore = false;
                }

                await _context.SaveChangesAsync();
            }
            

            return true;
        }

        public async Task<bool> RemoveFlagsByStateAsync(string selectedState)
        {
            var errorRecords = await _context.SubmittalErrors
                                  .Where(s => s.Ignore == true && s.SubmittedBy == selectedState)
                                  .ToListAsync();

            if (errorRecords.Any())
            {
                // Update Ignore to False for each record
                foreach (var record in errorRecords)
                {
                    record.Ignore = false;
                }

                await _context.SaveChangesAsync();
            }


            return true;
        }

        public async Task<bool> RemoveFlagsByDataSetAsync(string selectedDataSet)
        {
            var errorRecords = await _context.SubmittalErrors
                                  .Where(s => s.Ignore == true && s.DataSet == selectedDataSet)
                                  .ToListAsync();

            if (errorRecords.Any())
            {
                // Update Ignore to False for each record
                foreach (var record in errorRecords)
                {
                    record.Ignore = false;
                }

                await _context.SaveChangesAsync();
            }


            return true;
        }



    }
}
