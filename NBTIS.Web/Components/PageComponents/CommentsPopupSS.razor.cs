using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using NBTIS.Web.ViewModels;
using NBTIS.Core.DTOs;
using Telerik.Blazor.Components;
using NBTIS.Web.Services;
using NBTIS.Data.Models;
using Microsoft.Graph;
using Microsoft.AspNetCore.Components.Forms;

namespace NBTIS.Web.Components.PageComponents
{
    public partial class CommentsPopupSS : ComponentBase
    {
        [Inject]
        private SubmittalService _submittalService { get; set; }

        [Parameter]
        public long? EditItem_SubmitId { get; set; }

        public VSubmittalLog EditItem { get; set; }
        public List<SubmittalCommentDTO> EditItem_SubmittalComments { get; set; }

        [Parameter]
        public string? CurrentUser { get; set; } // Identifier for the current user

        [Parameter]
        public EventCallback OnValidSubmit { get; set; }

        [Parameter]
        public EventCallback OnCancel { get; set; }

        private string newCommentText { get; set; } = string.Empty;

        private bool isUpdateSuccessful = false;
        private string successMessage = string.Empty;

        public string errorMessage = string.Empty;

        public TelerikDialog DialogRefSS { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            errorMessage = "";
            if (EditItem_SubmitId != null)
            {
                await PageReload();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            var results = await _submittalService.Load_VsubmittalLog((long)EditItem_SubmitId);
            if (results != null)
            {
                EditItem = results;
            }
        }

        private async Task PageReload()
        {
           
            EditItem_SubmittalComments = await _submittalService.GetSubmittalCommentsSSAsync((long)EditItem_SubmitId);
        }

        private async Task AddComment()
        {

            if (!string.IsNullOrWhiteSpace(newCommentText))
            {
                var newComment = new SubmittalCommentDTO
                {
                    SubmitId = (long)EditItem_SubmitId,
                    CommentText = newCommentText,
                    CreatedBy = CurrentUser,
                    CreatedDate = DateTime.Now,
                    CommentType = NBTIS.Core.Utilities.Constants.CommentType_ACC_REJ,
                };
                newCommentText = string.Empty;
                SubmittalComment submittalComment=  await SaveNewCommentAsync(newComment);
                newComment.Id = submittalComment.Id;
                await PageReload();
            }
            else
            {
                errorMessage = "Please enter your comments";
                StateHasChanged();
                //DialogRefSS.Refresh();
            }
        }

        private async Task<SubmittalComment> SaveNewCommentAsync(SubmittalCommentDTO newComment)
        {
            // Persist the new comment directly from the child component.
            SubmittalComment submittalComment= await _submittalService.SaveNewCommentAsync(newComment);
            Console.WriteLine($"[Child] New comment added for SubmittalId {newComment.SubmitId} by {newComment.CreatedBy}.");
            isUpdateSuccessful = true;
            successMessage = "New Comment added successfully!";
            StateHasChanged();
            await Task.Delay(1000);
            // Reset after fade-out
            isUpdateSuccessful = false;
            successMessage = string.Empty;
            return submittalComment;
        }

        private async Task UpdateCommentAsync(SubmittalCommentDTO comment)
        {
            comment.UpdatedDate = DateTime.Now;

            await _submittalService.UpdateCommentAsync(comment);
            isUpdateSuccessful = true;
            successMessage = "Comment updated successfully!";
            StateHasChanged();
            await Task.Delay(1000);
            // Reset after fade-out
            isUpdateSuccessful = false;
            successMessage = string.Empty;

            // Update the comment directly from the child component.
            Console.WriteLine($"[Child] Comment updated for SubmittalId {comment.SubmitId} by {comment.CreatedBy}.");
            await PageReload();
            await Task.CompletedTask;
        }

        private async Task DeleteCommentAsync(SubmittalCommentDTO comment)
        {

            await _submittalService.DeleteCommentAsync(comment);
            //EditItem.SubmittalComments.Remove(comment);
            isUpdateSuccessful = true;
            successMessage = "Comment deleted successfully!";
            StateHasChanged();
            await Task.Delay(1000);
            // Reset after fade-out
            isUpdateSuccessful = false;
            successMessage = string.Empty;

            Console.WriteLine($"[Child] Comment deleted for SubmittalId {comment.SubmitId} by {comment.CreatedBy}.");
            await PageReload();
            StateHasChanged();
            await Task.CompletedTask;
        }
    }
}
