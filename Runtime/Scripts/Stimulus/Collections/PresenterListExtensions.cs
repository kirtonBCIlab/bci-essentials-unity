using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BCIEssentials.Stimulus
{
    using Extensions;

    public static class PresenterListExtensions
    {
        public static List<ISelectable> WhereSelectable
        (this IEnumerable<ISelectable> caller)
        => caller.Where(p => p.IsSelectable).ToList();
        public static List<StimulusPresenter> WhereSelectable
        (this IEnumerable<StimulusPresenter> caller)
        => caller.Where(p => p.IsSelectable).ToList();


        public static List<IStimulusPresenter> WhereVisibleFromMainCamera
        (this IEnumerable<IStimulusPresenter> caller)
        => caller.WhereVisibleFromCamera(Camera.main);
        public static List<StimulusPresenter> WhereVisibleFromMainCamera
        (this IEnumerable<StimulusPresenter> caller)
        => caller.WhereVisibleFromCamera(Camera.main);


        public static List<IStimulusPresenter> WhereVisibleFromCamera
        (this IEnumerable<IStimulusPresenter> caller, Camera camera)
        => caller.Where(p => p switch
            {
                Component c => c.gameObject.HasRendererVisibleFromCamera(camera),
                _ => true
            }
        ).ToList();

        public static List<StimulusPresenter> WhereVisibleFromCamera
        (this IEnumerable<StimulusPresenter> caller, Camera camera)
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
        (this IEnumerable<StimulusPresenter> caller)
        => caller.ToList().ForEach(p => p.StartStimulusDisplay());

        public static void EndStimulusDisplay
        (this IEnumerable<IStimulusPresenter> caller)
        => caller.ToList().ForEach(p => p.EndStimulusDisplay());

        public static void EndStimulusDisplay
        (this IEnumerable<StimulusPresenter> caller)
        => caller.ToList().ForEach(p => p.EndStimulusDisplay());
    }
}