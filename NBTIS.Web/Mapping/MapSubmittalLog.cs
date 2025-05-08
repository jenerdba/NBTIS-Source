using AutoMapper;
using NBTIS.Core.DTOs;
using NBTIS.Core.Enums;
using NBTIS.Data.Models;
using NBTIS.Web.ViewModels;

namespace NBTIS.Web.Mapping
{
    public class MapSubmittalLog : Profile
    {
        public MapSubmittalLog()
        {
            CreateMap<SubmittalLogDTO, SubmittalItem>()
                .ForMember(dest => dest.SubmitId, opt => opt.MapFrom(src => src.SubmitId))
                .ForMember(dest => dest.SubmittedBy, opt => opt.MapFrom(src => src.SubmittedBy))
                .ForMember(dest => dest.SubmittedByDescription, opt => opt.MapFrom(src => src.SubmittedByDescription))
                .ForMember(dest => dest.UploadType, opt => opt.MapFrom(src => src.IsPartial ? "Partial" : "Full"))
                .ForMember(dest => dest.UploadDate, opt => opt.MapFrom(src => src.UploadDate))
                .ForMember(dest => dest.StatusCode, opt => opt.MapFrom(src => src.StatusCode))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => GetStatusFromCode(src.StatusCode)))
                .ForMember(dest => dest.ReportContent, opt => opt.MapFrom(src => src.ReportContent ?? new byte[0]))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments ?? string.Empty))
                .ForMember(dest => dest.SubmitAllowed, opt => opt.MapFrom(src => GetStatusFromCode(src.StatusCode) == SubmittalStatus.New))
                .ForMember(dest => dest.CorrectAllowed, opt => opt.MapFrom(src => IsCorrectAllowed(src)))
                .ForMember(dest => dest.DeleteAllowed, opt => opt.MapFrom(src => IsDeleteAllowed(src)))
                .ForMember(dest => dest.CancelAllowed, opt => opt.MapFrom(src => IsCancelAllowed(src)))
                .ForMember(dest => dest.SubmittalComments, opt => opt.MapFrom(src => src.SubmittalComments));
            
            CreateMap<SubmissioniStatusItemViewModel, VSubmittalLog>().ReverseMap();
        }

        private object IsCancelAllowed(SubmittalLogDTO src)
        {
            var status = GetStatusFromCode(src.StatusCode);
            return status == SubmittalStatus.DivisionReview;
        }

        private SubmittalStatus GetStatusFromCode(byte statusCode)
        {
            // Check if the value is defined in the SubmittalStatus enum
            if (Enum.IsDefined(typeof(SubmittalStatus), (int)statusCode))
            {
                return ((SubmittalStatus)statusCode);
            }
            return SubmittalStatus.Pending;
        }

        private bool IsCorrectAllowed(SubmittalLogDTO src)
        {
            var status = GetStatusFromCode(src.StatusCode);
            return status == SubmittalStatus.New
                || status == SubmittalStatus.SubmitFailed
                || status == SubmittalStatus.ReturnedByDivision
                || status == SubmittalStatus.ValidationFailed;
        }

        private bool IsDeleteAllowed(SubmittalLogDTO src)
        {
            var status = GetStatusFromCode(src.StatusCode);
            return status == SubmittalStatus.Pending 
                || status == SubmittalStatus.New 
                || status == SubmittalStatus.SubmitFailed 
                || status == SubmittalStatus.Canceled 
                || status == SubmittalStatus.ReturnedByDivision
                || status == SubmittalStatus.ValidationFailed;
        }

    }
}
