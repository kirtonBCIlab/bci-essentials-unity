using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BCIEssentials.Stimulus.Collections
{
    using Extensions;
    using Stimulus.Presentation;

    public static class PresenterListExtensions
    {
        public static List<ISelectable> WhereSelectable
        (this IEnumerable<ISelectable> caller)
        => caller.Where(p => p.IsSelectable).ToList();
        public static List<StimulusPresentationBehaviour> WhereSelectable
        (this IEnumerable<StimulusPresentationBehaviour> caller)
        => caller.Where(p => p.IsSelectable).ToList();


        public static List<IStimulusPresenter> WhereVisibleFromMainCamera
        (this IEnumerable<IStimulusPresenter> caller)
        => caller.WhereVisibleFromCamera(Camera.main);
        public static List<StimulusPresentationBehaviour> WhereVisibleFromMainCamera
        (this IEnumerable<StimulusPresentationBehaviour> caller)
        => caller.WhereVisibleFromCamera(Camera.main);


        public static List<IStimulusPresenter> WhereVisibleFromCamera
        (this IEnumerable<IStimulusPresenter> caller, Camera camera)
        => caller.Where(p => p switch
            {
                Component c => c.gameObject.HasRendererVisibleFromCamera(camera),
                _ => true
            }
        ).ToList();

        public static List<StimulusPresentationBehaviour> WhereVisibleFromCamera
        (this IEnumerable<StimulusPresentationBehaviour> caller, Camera camera)
        => caller.Where(p => p switch
            {
                Component c => c.gameObject.HasRendererVisibleFromCamera(camera),
                _ => true
            }
        ).ToList();


        public static void StartStimulusDisplay
        (this IEnumerable<IStimulusPresenter> caller)
        => caller.ToList().ForEach(p => p.StartStimulusDisplay());

        public static void StartStimulusDisplay
        (this IEnumerable<StimulusPresentationBehaviour> caller)
        => caller.ToList().ForEach(p => p.StartStimulusDisplay());

        public static void EndStimulusDisplay
        (this IEnumerable<IStimulusPresenter> caller)
        => caller.ToList().ForEach(p => p.EndStimulusDisplay());

        public static void EndStimulusDisplay
        (this IEnumerable<StimulusPresentationBehaviour> caller)
        => caller.ToList().ForEach(p => p.EndStimulusDisplay());
    }
}