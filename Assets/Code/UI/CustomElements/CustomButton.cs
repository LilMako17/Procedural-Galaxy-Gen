// web* src: https://gist.github.com/andrew-raphael-lukasik/69c7858e39e22f197ca51b318b218cc7
using UnityEngine.UIElements;

[UnityEngine.Scripting.Preserve]
public class CustomButton : Button
{
    public bool enabled
    {
        get => enabledSelf;
        set => SetEnabled(value);
    }
    public new class UxmlFactory : UxmlFactory<CustomButton, UxmlTraits> { }
    public new class UxmlTraits : Button.UxmlTraits
    {
        UxmlBoolAttributeDescription enabledAttr = new UxmlBoolAttributeDescription { name = "enabled", defaultValue = true };
        public override void Init(VisualElement ve, IUxmlAttributes attributes, CreationContext context)
        {
            base.Init(ve, attributes, context);
            CustomButton instance = (CustomButton)ve;
            instance.enabled = enabledAttr.GetValueFromBag(attributes, context);
        }
    }
}