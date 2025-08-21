using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LicenseManager : MonoBehaviour
{
    private string filePath;

    [Header("만료 UI")]
    public GameObject EndCanvas; // 라이선스 만료 시 표시할 UI 캔버스

    [Header("리셋(재활용) UI 요소")]
    [SerializeField] private GameObject LicensePanel;   // LicensePanel
    [SerializeField] private GameObject ReusePanel;          // ReusePanel
    [SerializeField] private Button ReUseButton;              // ReUseButton
    [SerializeField] private TMP_InputField CodeInputField;   // CodeInputField
    [SerializeField] private Button CodeEnterButton;          // CodeEnterButton
    [SerializeField] private Button CloseCodePopupButton;     // CloseCodePopupButton

    [Header("리셋 코드")]
    [SerializeField] private string ResetCode = "123456";     // 인스펙터에서 수정 가능

    // ▼ 추가: 만료 기준을 '시간' 단위로 변경
    private float licenseDurationHours = 0.1f; // 기본 1년(8760시간) 365f * 24f

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, ".sys_lic.dat");

        // 연결이 비어 있으면 계층 이름으로 자동 바인딩 (요청하신 명칭 사용)
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

            /*double daysElapsed = (DateTime.UtcNow - savedDate).TotalDays;

            if (daysElapsed > 365)
            {
                Debug.Log("사용 기간이 만료되었습니다!");
                if (EndCanvas != null)
                {
                    var quitBtn = EndCanvas.transform.GetComponentInChildren<Button>();
                    if (quitBtn != null) quitBtn.onClick.AddListener(() => Application.Quit());
                    EndCanvas.SetActive(true);
                }
            }
            else
            {
                int daysLeft = 365 - (int)daysElapsed;
                Debug.Log($"dat 파일 내용: {savedDateStr}");
                Debug.Log($"남은 사용일: {daysLeft}일");
                if (EndCanvas != null) EndCanvas.SetActive(false); // 유효하면 만료 UI 숨김
            }*/

            // 변경: '일'이 아니라 '시간' 단위로 경과 시간 계산
            TimeSpan elapsed = DateTime.UtcNow - savedDate;
            double hoursElapsed = elapsed.TotalHours;

            // ▼ 변경: 하드코딩(0.1h)이 아닌 변수(licenseDurationHours)로 만료 판단
            if (hoursElapsed > licenseDurationHours)
            {
                Debug.Log("사용 기간이 만료되었습니다! (변경 기준 적용)");
                if (EndCanvas != null)
                {
                    Button quitBtn = EndCanvas.transform.GetComponentInChildren<Button>();
                    if (quitBtn != null)
                        quitBtn.onClick.AddListener(() => Application.Quit()); // 버튼 클릭 시 앱 종료
                    EndCanvas.SetActive(true); // 라이선스 만료 UI 활성화
                }
            }
            else
            {
                // ▼ 변경: 설정한 만료 시간 기준으로 남은 시간 계산/출력
                TimeSpan remain = TimeSpan.FromHours(licenseDurationHours) - elapsed;

                // UI에 남은 시간 출력 (TextMeshPro)
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
    }

    private void OnClickEnterCode()
    {
        string input = CodeInputField != null ? CodeInputField.text?.Trim() : string.Empty;

        if (string.IsNullOrEmpty(input))
        {
            Debug.LogWarning("코드를 입력하세요.");
            return;
        }

        if (input == ResetCode)
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
        }
    }

    private void AutoBindIfNull()
    {
        // 계층 이름 기반 자동 찾기 (이미 드래그 연결했다면 무시)
        if (ReusePanel == null) ReusePanel = GameObject.Find("ReusePanel");
        if (ReUseButton == null)
        {
            var go = GameObject.Find("ReUseButton") ?? GameObject.Find("ReuseButton"); // 오타 대비
            if (go != null) ReUseButton = go.GetComponent<Button>();
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
}
