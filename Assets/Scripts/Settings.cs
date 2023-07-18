using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Settings : Page
{
    #region Private Variables

    private bool            canClick;

    private VisualElement   mainContainer;
    private VisualElement   mainRibbon;

    private VisualElement   settings_Music;
    private VisualElement   settings_SFX;
    private VisualElement   settings_ColorSettingsButton;
    private VisualElement   settings_CloseButton;

    private VisualElement   colorSetter_Container;
    private VisualElement   colorSetter_colorDisplay;
    private Slider          colorSetter_redSlider;
    private Slider          colorSetter_blueSlider;
    private Slider          colorSetter_greenSlider;
    private TextElement     colorSetter_colorName;
    private int             colorSetter_currentIndex;

    private ScrollView      colorSettings_ColorListScroll;
    private VisualElement   colorSettings_CloseButton;
    private List<Label>     colorSettings_ColorNumberLabels;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        mainContainer = uiDoc.rootVisualElement.Q<VisualElement>("Container");

        SetupMainSettings();
        SetupColorEditor();
        SetupColorList();
    }

    public override IEnumerator AnimateIn()
    {
        VisualElement page  = uiDoc.rootVisualElement;

        page.SetPadding(0f);
        page.style.opacity  = new StyleFloat(0f);

        Tween fadein        = DOTween.To(() => page.style.opacity.value,
                                x => page.style.opacity = new StyleFloat(x),
                                1f, .33f);

        yield return fadein.Play().WaitForCompletion();

        canClick = true;
    }

    public override IEnumerator AnimateOut()
    {
        yield return null;
    }

    public override void HidePage()
    {
        return;
    }

    #endregion

    #region Private Functions

    private void SetupMainSettings()
    {
        settings_Music                  = mainContainer.Q<VisualElement>("MusicPlaceholder");
        settings_SFX                    = mainContainer.Q<VisualElement>("SFXPlaceholder");
        settings_CloseButton            = mainContainer.Q<VisualElement>("CancelButton");
        settings_ColorSettingsButton    = mainContainer.Q<VisualElement>("ColorSettingsButton");

        settings_CloseButton.RegisterCallback<PointerUpEvent>((evt) =>
        {
            canClick = false;

            PageManager.instance.StartCoroutine(PageManager.instance.CloseTopPage());
        });

        uiDoc.rootVisualElement.Q<VisualElement>("TopBlankSpace").RegisterCallback<PointerUpEvent>((evt) =>
        {
            canClick = false;

            PageManager.instance.StartCoroutine(PageManager.instance.CloseTopPage());
        });

        uiDoc.rootVisualElement.Q<VisualElement>("BottomBlankSpace").RegisterCallback<PointerUpEvent>((evt) =>
        {
            canClick = false;

            PageManager.instance.StartCoroutine(PageManager.instance.CloseTopPage());
        });

        settings_ColorSettingsButton.RegisterCallback<PointerUpEvent>(evt => ShowHideColorSettings(evt, true));
    }

    private void SetupColorList()
    {
        VisualElement colorSettings_Container       = UIManager.instance.Settings_ColorList.Instantiate();
        Toggle showNumsToggle                       = colorSettings_Container.Q<Toggle>("ShowColorNumbersToggle");
        colorSettings_ColorListScroll               = colorSettings_Container.Q<ScrollView>();
        colorSettings_CloseButton                   = colorSettings_Container.Q<VisualElement>("CancelButton");

        mainContainer.Add(colorSettings_ColorListScroll);
        mainContainer.Add(colorSettings_CloseButton);

        showNumsToggle.value                        = true; //TODO: have this set from a saved value
        showNumsToggle.focusable                    = false;
        showNumsToggle.labelElement.AddToClassList("HeaderLabel");
        showNumsToggle.labelElement.style.fontSize  = 45f;
        showNumsToggle.SetMargins(0f, false, true, false, true);
        showNumsToggle.style.marginBottom           = 25f;

        VisualElement check                         = showNumsToggle.Q<VisualElement>("unity-checkmark");
        check.parent.style.justifyContent           = Justify.FlexEnd;
        check.SetColor(new Color(.8f, .8f, .8f, 1f));
        check.SetBorderWidth(0f);
        check.SetBorderRadius(20f);
        check.ScaleToFit();
        check.SetWidth(70f);
        check.SetHeight(check.style.width);

        colorSettings_ColorNumberLabels             = new List<Label>();
        ColorCategory prevCat                       = ColorCategory.SYSTEM;

        for (int i = 0; i < UIManager.instance.ColorCount; i++)
        {
            GameColor gameCol = UIManager.instance.GetGameColor(i);

            if (i == 0 || gameCol.category != prevCat)
            {
                Label catLabel                      = new Label();
                catLabel.AddToClassList("HeaderLabel");
                catLabel.SetMargins(0f);
                catLabel.style.marginTop            = 15f;
                catLabel.style.marginBottom         = 25f;
                catLabel.style.unityTextAlign       = TextAnchor.MiddleLeft;
                catLabel.SetBorderColor(Color.black);
                catLabel.SetBorderWidth(5f, false, false, true, false);
                catLabel.text                       = gameCol.category.Name() + " ***";
                colorSettings_ColorListScroll.Add(catLabel);
            }

            prevCat                                 = gameCol.category;

            VisualElement colorLine                 = UIManager.instance.Settings_ColorLine.Instantiate();
            int colorIndex                          = i;

            colorLine.Q<VisualElement>("ColorBox").SetColor(gameCol.color);
            colorLine.Q<Label>("ColorName").text    = gameCol.name;

            Label num                               = colorLine.Q<Label>("Number");
            num.text                                = colorIndex.ToString();
            colorSettings_ColorNumberLabels.Add(num);
            num.Show(showNumsToggle.value);

            colorLine.userData                      = colorIndex;

            colorSettings_ColorListScroll.Add(colorLine);

            colorLine.RegisterCallback<PointerUpEvent>((evt) => ShowColorEditor(evt, colorIndex));
        }

        showNumsToggle.RegisterValueChangedCallback<bool>((evt) => ShowHideColorNumbers(evt.newValue));
        colorSettings_CloseButton.RegisterCallback<PointerUpEvent>(evt => ShowHideColorSettings(evt, false));

        colorSettings_ColorListScroll.Hide();
        colorSettings_CloseButton.Hide();
    }

    private void SetupColorEditor()
    {
        colorSetter_Container       = UIManager.instance.Settings_ColorSetter.Instantiate().Q<VisualElement>("Page");
        uiDoc.rootVisualElement.Q<VisualElement>("Page").Add(colorSetter_Container);

        TextField nameTF            = colorSetter_Container.Q<TextField>("NameTextField");
        nameTF.Q<VisualElement>("unity-text-input").RemoveFromClassList("unity-text-input");
        nameTF.Q<VisualElement>("unity-text-input").AddToClassList("TextInputContainer");

        colorSetter_colorName       = nameTF.Q<TextElement>();
        colorSetter_colorName.RemoveFromClassList("unity-text-element");
        colorSetter_colorName.AddToClassList("TextInputTextElement");

        colorSetter_colorDisplay    = colorSetter_Container.Q<VisualElement>("ColorDisplay");

        colorSetter_redSlider       = colorSetter_Container.Q<VisualElement>("Red").Q<Slider>();
        colorSetter_blueSlider      = colorSetter_Container.Q<VisualElement>("Blue").Q<Slider>();
        colorSetter_greenSlider     = colorSetter_Container.Q<VisualElement>("Green").Q<Slider>();

        TextField redTF             = colorSetter_redSlider.Q<TextField>();
        VisualElement redTextCont   = redTF.Q<VisualElement>("unity-text-input");
        TextElement redTE           = colorSetter_redSlider.Q<TextElement>();
        VisualElement redDragCont   = colorSetter_redSlider.Q<VisualElement>("unity-drag-container");
        VisualElement redDragLine   = colorSetter_redSlider.Q<VisualElement>("unity-tracker");
        VisualElement redDragger    = colorSetter_redSlider.Q<VisualElement>("unity-dragger");

        TextField greenTF           = colorSetter_greenSlider.Q<TextField>();
        VisualElement greenTextCont = greenTF.Q<VisualElement>("unity-text-input");
        TextElement greenTE         = colorSetter_greenSlider.Q<TextElement>();
        VisualElement greenDragCont = colorSetter_greenSlider.Q<VisualElement>("unity-drag-container");
        VisualElement greenDragLine = colorSetter_greenSlider.Q<VisualElement>("unity-tracker");
        VisualElement greenDragger  = colorSetter_greenSlider.Q<VisualElement>("unity-dragger");

        TextField blueTF            = colorSetter_blueSlider.Q<TextField>();
        VisualElement blueTextCont  = blueTF.Q<VisualElement>("unity-text-input");
        TextElement blueTE          = colorSetter_blueSlider.Q<TextElement>();
        VisualElement blueDragCont  = colorSetter_blueSlider.Q<VisualElement>("unity-drag-container");
        VisualElement blueDragLine  = colorSetter_blueSlider.Q<VisualElement>("unity-tracker");
        VisualElement blueDragger   = colorSetter_blueSlider.Q<VisualElement>("unity-dragger");

        Color bgGrey                = new Color(.8f, .8f, .8f, 1f);

        redTF.SetWidth(175f);
        redDragCont.SetMargins(25f, false, true, false, false);
        redDragLine.style.right     = 10f;
        redDragLine.style.left      = redDragLine.style.right;
        redDragLine.SetBorderWidth(0f);
        redDragLine.SetHeight(10f);
        redDragLine.SetMargins(-5f, true, false, false, false);
        redDragger.SetBorderWidth(0f);
        redDragger.SetBorderRadius(20f);
        redDragger.SetWidth(40f);
        redDragger.SetHeight(redDragger.style.width);
        redDragger.SetMargins(-20f, true, false, false, false);
        redDragger.SetColor(bgGrey);

        greenTF.SetWidth(175f);
        greenDragCont.SetMargins(25f, false, true, false, false);
        greenDragLine.style.right = 10f;
        greenDragLine.style.left = greenDragLine.style.right;
        greenDragLine.SetBorderWidth(0f);
        greenDragLine.SetHeight(10f);
        greenDragLine.SetMargins(-5f, true, false, false, false);
        greenDragger.SetBorderWidth(0f);
        greenDragger.SetBorderRadius(20f);
        greenDragger.SetWidth(40f);
        greenDragger.SetHeight(greenDragger.style.width);
        greenDragger.SetMargins(-20f, true, false, false, false);
        greenDragger.SetColor(bgGrey);

        blueTF.SetWidth(175f);
        blueDragCont.SetMargins(25f, false, true, false, false);
        blueDragLine.style.right = 10f;
        blueDragLine.style.left = blueDragLine.style.right;
        blueDragLine.SetBorderWidth(0f);
        blueDragLine.SetHeight(10f);
        blueDragLine.SetMargins(-5f, true, false, false, false);
        blueDragger.SetBorderWidth(0f);
        blueDragger.SetBorderRadius(20f);
        blueDragger.SetWidth(40f);
        blueDragger.SetHeight(blueDragger.style.width);
        blueDragger.SetMargins(-20f, true, false, false, false);
        blueDragger.SetColor(bgGrey);

        redTextCont.RemoveFromClassList("unity-text-input");
        redTextCont.AddToClassList("TextInputContainer");
        redTE.RemoveFromClassList("unity-text-element");
        redTE.AddToClassList("TextInputTextElement");

        greenTextCont.RemoveFromClassList("unity-text-input");
        greenTextCont.AddToClassList("TextInputContainer");
        greenTE.RemoveFromClassList("unity-text-element");
        greenTE.AddToClassList("TextInputTextElement");

        blueTextCont.RemoveFromClassList("unity-text-input");
        blueTextCont.AddToClassList("TextInputContainer");
        blueTE.RemoveFromClassList("unity-text-element");
        blueTE.AddToClassList("TextInputTextElement");

        colorSetter_redSlider.RegisterCallback<ChangeEvent<float>>(ColorEditorSlidersChanged);
        colorSetter_greenSlider.RegisterCallback<ChangeEvent<float>>(ColorEditorSlidersChanged);
        colorSetter_blueSlider.RegisterCallback<ChangeEvent<float>>(ColorEditorSlidersChanged);

        colorSetter_Container.Q<VisualElement>("SaveButton").RegisterCallback<PointerUpEvent>(SaveColorSetter);
        colorSetter_Container.Q<VisualElement>("CancelButton").RegisterCallback<PointerUpEvent>(CancelColorSetter);

        colorSetter_Container.Hide();
    }

    private void ShowHideColorSettings(PointerUpEvent evt, bool show)
    {
        settings_Music.Show(!show);
        settings_SFX.Show(!show);
        settings_CloseButton.Show(!show);
        settings_ColorSettingsButton.Show(!show);

        mainContainer.SetHeight(new StyleLength(new Length(show ? 75f : 50f, LengthUnit.Percent)));

        colorSettings_ColorListScroll.Show(show);
        colorSettings_CloseButton.Show(show);
    }

    private void ShowColorEditor(PointerUpEvent evt, int colorIndex)
    {
        mainContainer.Hide();

        Color c                     = UIManager.instance.GetColor(colorIndex);

        colorSetter_colorDisplay.SetColor(c);

        colorSetter_colorName.text  = UIManager.instance.GetColorName(colorIndex);

        colorSetter_redSlider.SetValueWithoutNotify(c.r * 255f);
        colorSetter_blueSlider.SetValueWithoutNotify(c.b * 255f);
        colorSetter_greenSlider.SetValueWithoutNotify(c.g * 255f);

        colorSetter_currentIndex    = colorIndex;

        colorSetter_Container.Show();
    }

    private void ColorEditorSlidersChanged(ChangeEvent<float> evt)
    {
        Color newColor = new Color(
            colorSetter_redSlider.value / 255f
            , colorSetter_greenSlider.value / 255f
            , colorSetter_blueSlider.value / 255f
            , 1f);

        colorSetter_colorDisplay.SetColor(newColor);
    }

    private void CancelColorSetter(PointerUpEvent evt)
    {
        colorSetter_Container.Hide();
        mainContainer.Show();
    }

    private void SaveColorSetter(PointerUpEvent evt)
    {
        Color newColor = new Color(
            colorSetter_redSlider.value / 255f
            , colorSetter_greenSlider.value / 255f
            , colorSetter_blueSlider.value / 255f
            , 1f);

        UIManager.instance.UpdateColor(
            colorSetter_currentIndex
            , colorSetter_colorName.text
            , newColor);

        VisualElement colorLine = colorSettings_ColorListScroll.contentContainer.Children().First(x => x.userData is int && (int)x.userData == colorSetter_currentIndex);
        colorLine.Q<VisualElement>("ColorBox").SetColor(newColor);
        colorLine.Q<Label>("ColorName").text = colorSetter_colorName.text;

        CancelColorSetter(evt);
    }

    private void ShowHideColorNumbers(bool show)
    {
        for (int i = 0; i < colorSettings_ColorNumberLabels.Count; i++)
            colorSettings_ColorNumberLabels[i].Show(show);
    }

    #endregion
}
