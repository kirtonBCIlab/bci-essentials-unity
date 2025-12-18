using System.Collections.Generic;
using System.Linq;
using BCIEssentials.Stimulus.Presentation;
using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.Stimulus.Collections
{
    public static class PresenterListExtensions
    {
        public static List<IStimulusPresenter> WhereSelectable
        (this IEnumerable<IStimulusPresenter> caller)
        => caller.Where(p => p.IsSelectable).ToList();

        public static List<IStimulusPresenter> WhereVisibleFromMainCamera
        (this IEnumerable<IStimulusPresenter> caller)
        => caller.WhereVisibleFromCamera(Camera.main);

        public static List<IStimulusPresenter> WhereVisibleFromCamera
        (this IEnumerable<IStimulusPresenter> caller, Camera camera)
        => caller.Where(p => p switch
            {
                Component c => c.gameObject.HasRendererVisibleFromCamera(camera),
                _ => true
            }
        ).ToList();


        public static void StartStimulusDisplay
        (this IEnumerable<IStimulusPresenter> caller)
        => caller.ToList().ForEach(p => p.StartStimulusDisplay());
        public static void EndStimulusDisplay
        (this IEnumerable<IStimulusPresenter> caller)
        => caller.ToList().ForEach(p => p.EndStimulusDisplay());
    }
}