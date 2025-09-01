using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LicenseManager : MonoBehaviour
{
    private string filePath;

    [Header("만료 UI")]
    public GameObject EndCanvas; // 라이선스 만료 시 표시할 UI 캔버스

    [Header("리셋(재활용) UI 요소")]
    [SerializeField] private GameObject LicensePanel;   // LicensePanel
    [SerializeField] private GameObject ReusePanel;          // ReusePanel
    [SerializeField] private Button ReUseButton;              // ReUseButton, green, right, 재활성화
    [SerializeField] private Button QuitButton;         // QuitButton, red, left, 종료
    [SerializeField] private TMP_InputField CodeInputField;   // CodeInputField, '코드를 입력하세요...', max length = 8, type : pin
    [SerializeField] private Button CodeEnterButton;          // CodeEnterButton,  blue, 입력
    [SerializeField] private Button CloseCodePopupButton;     // CloseCodePopupButton, red, 닫기

    private float licenseDurationHours = 8760f; // 라이센스 제공 기간, 1 = 1h, 1년(8760시간)

    void Start()
    {
        //숨겨진 데이터 파일 경로 설정
        filePath = Path.Combine(Application.persistentDataPath, ".sys_lic.dat");

        // 연결이 비어 있으면 계층 이름으로 자동 바인딩
        AutoBindIfNull();

        // 패널 기본 비활성화
        if (ReusePanel != null) ReusePanel.SetActive(false);

        // 버튼 리스너 등록
        WireUpButtons();

        try
        {
            if (!File.Exists(filePath))
            {
                // 최초 실행일 저장 (UTC, ISO 8601)
                File.WriteAllText(filePath, DateTime.UtcNow.ToString("o"));
                Debug.Log("최초 실행일 저장 완료");
            }

            // 상태 적용
            ApplyLicenseStatus();
        }
        catch (Exception ex)
        {
            Debug.LogError($"라이선스 파일 처리 중 오류 발생: {ex.Message}");
        }
    }

    private void ApplyLicenseStatus()
    {
        try
        {
            string savedDateStr = File.ReadAllText(filePath);
            DateTime savedDate = DateTime.Parse(savedDateStr, null, System.Globalization.DateTimeStyles.RoundtripKind);

            // 시간 단위로 경과 시간 계산
            TimeSpan elapsed = DateTime.UtcNow - savedDate;
            double hoursElapsed = elapsed.TotalHours;

            // licenseDurationHours로 만료 판단
            if (hoursElapsed > licenseDurationHours)
            {
                Debug.Log("사용 기간이 만료되었습니다! (변경 기준 적용)");
                if (EndCanvas != null)
                {
                    EndCanvas.SetActive(true); // 라이선스 만료 UI 활성화
                }
            }
            else
            {
                // 설정한 만료 시간 기준으로 남은 시간 계산/출력
                TimeSpan remain = TimeSpan.FromHours(licenseDurationHours) - elapsed;

                // 디버그 로그 출력
                string remainText = $"남은 시간: {Mathf.Max(0, remain.Days)}일 {Mathf.Max(0, remain.Hours)}시간 {Mathf.Max(0, remain.Minutes):00}분 {Mathf.Max(0, remain.Seconds):00}초";
                Debug.Log("dat 파일 내용: " + savedDateStr);
                Debug.Log(remainText);

                if (EndCanvas != null) EndCanvas.SetActive(false); // 유효하면 만료 UI 숨김
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"상태 적용 중 오류: {ex.Message}");
        }
    }


    // 코드 입력 후 확인버튼 처리
    private void OnClickEnterCode()
    {
        string input = CodeInputField != null ? CodeInputField.text?.Trim() : string.Empty;

        if (string.IsNullOrEmpty(input))
        {
            Debug.LogWarning("코드를 입력하세요.");
            return;
        }

        if (input == GetTodayCodeKST())
        {
            try
            {
                // 현재 UTC로 데이터 파일 덮어쓰기 (초기화)
                string nowUtc = DateTime.UtcNow.ToString("o");
                File.WriteAllText(filePath, nowUtc);
                Debug.Log("라이선스가 현재 날짜로 초기화되었습니다.");

                // 패널 닫기
                if (ReusePanel != null) ReusePanel.SetActive(false);
                if (LicensePanel != null) LicensePanel.SetActive(false);

                // 상태 재적용
                ApplyLicenseStatus();
            }
            catch (Exception ex)
            {
                Debug.LogError($"라이선스 초기화 실패: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("코드가 올바르지 않습니다.");
            // 입력 실패 시 InputField 비우기
            if (CodeInputField != null) CodeInputField.text = string.Empty;
            Handheld.Vibrate();
            // ReusePanel을 DOTween으로 흔들기(삐빅 느낌)
            if (ReusePanel != null)
            {
                var rt = ReusePanel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // 중복 트윈 방지
                    rt.DOKill(true);
                    // 좌우 흔들림
                    rt.DOShakeAnchorPos(0.4f, new Vector2(20f, 0f), 20, 90f);
                }
                else
                {
                    // UI가 아니라 일반 Transform일 경우 포지션 흔들기
                    ReusePanel.transform.DOKill(true);
                    ReusePanel.transform.DOShakePosition(0.4f, new Vector2(20f, 0f), 20, 90f);
                }
            }
        }
    }

    // 자동 바인딩 메서드: 연결이 비어 있으면 계층 이름으로 자동 바인딩
    private void AutoBindIfNull()
    {
        if (ReusePanel == null) ReusePanel = GameObject.Find("ReusePanel");
        if (ReUseButton == null)
        {
            var go = GameObject.Find("ReUseButton") ?? GameObject.Find("ReuseButton"); // 오타 대비
            if (go != null) ReUseButton = go.GetComponent<Button>();
        }
        if (QuitButton == null)
        {
            var go = GameObject.Find("QuitButton");
            if (go != null) QuitButton = go.GetComponent<Button>();
        }
        if (CodeInputField == null)
        {
            var go = GameObject.Find("CodeInputField");
            if (go != null) CodeInputField = go.GetComponent<TMP_InputField>();
        }
        if (CodeEnterButton == null)
        {
            var go = GameObject.Find("CodeEnterButton");
            if (go != null) CodeEnterButton = go.GetComponent<Button>();
        }
        if (CloseCodePopupButton == null)
        {
            var go = GameObject.Find("CloseCodePopupButton");
            if (go != null) CloseCodePopupButton = go.GetComponent<Button>();
        }
        if (LicensePanel == null)
        {
            var go = GameObject.Find("LicensePanel");
            if (go != null) LicensePanel = go;
        }
    }

    //버튼 리스너 설정
    private void WireUpButtons()
    {
        if (ReUseButton != null)
            ReUseButton.onClick.AddListener(() =>
            {
                if (ReusePanel != null) ReusePanel.SetActive(true);
            });

        if (CloseCodePopupButton != null)
            CloseCodePopupButton.onClick.AddListener(() =>
            {
                if (ReusePanel != null) ReusePanel.SetActive(false);
            });

        if (CodeEnterButton != null)
            CodeEnterButton.onClick.AddListener(OnClickEnterCode);
        if (QuitButton != null)
        {
            QuitButton.onClick.AddListener(() => {
                Debug.Log("종료 버튼 눌림");
                Application.Quit();
            });
        }
    }

    // 한국 날짜 기반 일일 코드 생성 규칙 (한국 날짜 8자리에 각 숫자 자리수 i만큼 뒤로 이동)
    private string GetTodayCodeKST()
    {
        // UTC+9(표준시간+9시간) 직접 계산
        DateTime nowKST = DateTime.UtcNow.AddHours(9);

        string dateStr = nowKST.ToString("yyyyMMdd");   // 오늘 날짜 자동으로 string으로 변환 (예: 20250822)
        char[] outDigits = new char[dateStr.Length];    // 출력용 문자 배열

        for (int i = 0; i < dateStr.Length; i++)
        {
            int digit = dateStr[i] - '0';      // 0~9
            int shifted = digit - (i + 1);     // 자리수(1부터)만큼 뒤로 이동
            shifted = ((shifted % 10) + 10) % 10; // 음수 안전 모듈러(0~9로 순환)
            outDigits[i] = (char)('0' + shifted);
        }

        return new string(outDigits);
    }
}
