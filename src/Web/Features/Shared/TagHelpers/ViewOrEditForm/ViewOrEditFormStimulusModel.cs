using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Shared.TagHelpers.ViewOrEditForm;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record ViewOrEditFormStimulusModel(bool StartInEditMode) : BaseStimulusModel;
