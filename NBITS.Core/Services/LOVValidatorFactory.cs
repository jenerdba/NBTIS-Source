using NBTIS.Data.Models;

namespace NBTIS.Core.Services
{
    public class LOVValidatorFactory
    {
        private readonly DataContext _context;

        public LOVValidatorFactory()
        {
        }

        // DataContext injected via constructor
        public LOVValidatorFactory(DataContext context)
        {
            _context = context;
        }

        // Create or retrieve the relevant validator from the static cache
        public LOVValidatorsService Create(string lookupName)
        {
            return LOVValidatorsService.GetValidator(_context, lookupName);
        }
    }
}

