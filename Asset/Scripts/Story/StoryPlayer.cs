using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using DG.Tweening;

public class StoryPlayer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image storyImage;
    [SerializeField] private TMP_Text storyText;

    [Header("设置")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private float imageFadeTime = 0.25f;

    [Header("打字音效")]
    [SerializeField] private AudioSource typingSfx;

    private StoryData currentStory;

    private int index;
    private bool isTyping;

    private Coroutine typingCoroutine;
    private Coroutine autoCoroutine;

    private Tween imageTween;

    private Action onFinished;


    public void Initialize(
    Image image,
    TMP_Text text)
    {
        storyImage = image;
        storyText = text;

        if (storyImage != null)
        {
            storyImage.preserveAspect = true;
        }
    }

    public void Play(
    StoryData storyData,
    Image image,
    TMP_Text text,
    System.Action finishCallback)
    {
        Initialize(image, text);

        currentStory = storyData;
        onFinished = finishCallback;
        index = 0;

        // 提前设置第一张图片并设为透明，避免默认占位图闪烁
        if (storyImage != null && currentStory.nodes.Length > 0)
        {
            imageTween?.Kill();
            storyImage.sprite = currentStory.nodes[0].image;

            Color c = storyImage.color;
            c.a = 0f;
            storyImage.color = c;
        }

        ShowNode(0, isFirstNode: true);
    }

    private void ShowNode(int i, bool isFirstNode = false)
    {
        if (i < 0 || i >= currentStory.nodes.Length)
            return;

        StoryNode node = currentStory.nodes[i];

        imageTween?.Kill();

        if (isFirstNode)
        {
            // 已在 Play() 中设置好 sprite，直接淡入即可
            imageTween = storyImage.DOFade(1, imageFadeTime);
        }
        else
        {
            imageTween = storyImage
                .DOFade(0, imageFadeTime)
                .OnComplete(() =>
                {
                    storyImage.sprite = node.image;
                    storyImage.DOFade(1, imageFadeTime);
                });
        }

        StopTypingSfxImmediately();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(node.text));

        if (autoCoroutine != null)
            StopCoroutine(autoCoroutine);

        if (node.autoNext)
        {
            autoCoroutine = StartCoroutine(AutoNext(node.autoNextTime));
        }
    }

    private void Update()
    {
        if (currentStory == null)
            return;

        if (Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            OnClick();
        }
    }

    private void OnClick()
    {
        if (isTyping)
            return;

        NextNode();
    }

    private void ShowNode(int i)
    {
        if (i < 0 || i >= currentStory.nodes.Length)
            return;

        if (storyImage == null || storyText == null)
        {
            Debug.LogError("StoryPlayer: storyImage 或 storyText 为空，剧情无法播放。");
            return;
        }

        if (!storyText.gameObject.activeInHierarchy)
        {
            Debug.LogError("StoryPlayer: storyText 所在的物体未激活，TMP textInfo 不可用。");
            return;
        }

        StoryNode node = currentStory.nodes[i];

        imageTween?.Kill();

        imageTween = storyImage
            .DOFade(0, imageFadeTime)
            .OnComplete(() =>
            {
                storyImage.sprite = node.image;
                storyImage.DOFade(1, imageFadeTime);
            });

        StopTypingSfxImmediately();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine =
            StartCoroutine(TypeText(node.text));

        if (autoCoroutine != null)
            StopCoroutine(autoCoroutine);

        if (node.autoNext)
        {
            autoCoroutine =
                StartCoroutine(
                    AutoNext(node.autoNextTime));
        }
    }

    private IEnumerator TypeText(string content)
    {
        isTyping = true;

        storyText.text = content;
        storyText.ForceMeshUpdate();

        storyText.maxVisibleCharacters = 0;

        int totalCharacters =
            storyText.textInfo.characterCount;

        if (totalCharacters > 0)
        {
            if (typingSfx != null)
            {
                AudioManager.Instance.PlaySFX(typingSfx);
            }
        }

        for (int i = 0; i < totalCharacters; i++)
        {
            storyText.maxVisibleCharacters = i + 1;

            yield return new WaitForSeconds(typeSpeed);
        }

        StopTypingSfxImmediately();

        isTyping = false;
    }

    private IEnumerator AutoNext(float time)
    {
        yield return new WaitForSeconds(time);

        if (!isTyping)
        {
            NextNode();
        }
    }

    private void NextNode()
    {
        index++;

        if (index >= currentStory.nodes.Length)
        {
            EndStory();
            return;
        }

        ShowNode(index);
    }

    private void EndStory()
    {
        StopTypingSfxImmediately();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (autoCoroutine != null)
            StopCoroutine(autoCoroutine);

        imageTween?.Kill();

        storyText.text = "";
        storyImage.sprite = null;

        onFinished?.Invoke();

        Destroy(gameObject);
    }

    private void StopTypingSfxImmediately()
    {
        if (AudioManager.Instance != null &&
            typingSfx != null)
        {
            AudioManager.Instance.StopSFX(typingSfx);
        }
    }

    private void OnDestroy()
    {
        StopTypingSfxImmediately();

        imageTween?.Kill();
    }
}