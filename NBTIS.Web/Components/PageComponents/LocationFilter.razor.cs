using Microsoft.AspNetCore.Components;
using NBTIS.Web.ViewModels;
using Telerik.Blazor.Components;
using Telerik.DataSource;

namespace NBTIS.Web.Components.PageComponents
{
    public partial class LocationFilter
    {
        [Parameter]
        public List<TreeItem> FlatData { get; set; }


        [Parameter]
        public FilterMenuTemplateContext Context { get; set; }

        [Parameter]
        public bool ShowApplyButton { get; set; } = true;

        [Parameter]
        public EventCallback OnApply { get; set; }

        public IEnumerable<object> ExpandedItems { get; set; } = new List<object>();

     
        private IEnumerable<object> _checkedItems = new List<object>();

        [Parameter]
        public IEnumerable<object> SelectedItems { get; set; } = new List<object>();

        [Parameter]
        public EventCallback<IEnumerable<object>> SelectedItemsChanged { get; set; }
     
        private async Task ApplySelectedFilter()
        {
            Console.WriteLine("ApplySelectedFilter() was called.");
            if (OnApply.HasDelegate)
            {
                await OnApply.InvokeAsync();
            }
        }

        protected override void OnParametersSet()
        {
            var selectedValues = Context?.FilterDescriptor?.FilterDescriptors?
                .OfType<FilterDescriptor>()
                .Where(f => f.Operator == FilterOperator.IsEqualTo && f.Value is string)
                .Select(f => f.Value.ToString())
                .ToHashSet();

            if (selectedValues == null || selectedValues.Count == 0)
            {
                _checkedItems = new List<object>();
                return;
            }

            var matchedItems = FlatData
                .Where(item => selectedValues.Contains(item.Text))
                .Cast<object>()
                .ToList();

            // Handle special case: "All States" (Id = 1) and "All Agencies" (Id = 2)
            matchedItems = TryAddParentIfAllChildrenSelected(matchedItems, parentId: 1, selectedValues);
            matchedItems = TryAddParentIfAllChildrenSelected(matchedItems, parentId: 2, selectedValues);

            _checkedItems = matchedItems;
        }

        private List<object> TryAddParentIfAllChildrenSelected(List<object> matchedItems, int parentId, HashSet<string> selectedTexts)
        {
            var children = FlatData.Where(x => x.ParentId == parentId).ToList();
            var childTexts = children.Select(x => x.Text).ToHashSet();

            if (childTexts.All(t => selectedTexts.Contains(t)))
            {
                var parent = FlatData.FirstOrDefault(x => x.Id == parentId);
                if (parent != null)
                {
                    matchedItems.Add(parent);
                }
            }

            return matchedItems;
        }

        private async Task OnCheckedItemsChanged(IEnumerable<object> items)
        {
            _checkedItems = items;
            await SelectedItemsChanged.InvokeAsync(items);

            var compositeFilter = Context.FilterDescriptor;
            compositeFilter.FilterDescriptors.Clear();
            compositeFilter.LogicalOperator = FilterCompositionLogicalOperator.Or;

            var leafNodes = _checkedItems
                .OfType<TreeItem>()
                .Where(x => x.ParentId.HasValue)
                .Select(x => x.Text)
                .ToList();

            foreach (var value in leafNodes)
            {
                compositeFilter.FilterDescriptors.Add(new FilterDescriptor
                {
                    Member = nameof(SubmissioniStatusItemViewModel.Lookup_States_Description),
                    MemberType = typeof(string),
                    Operator = FilterOperator.IsEqualTo,
                    Value = value
                });
            }
        }
    }
}
