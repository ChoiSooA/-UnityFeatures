using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LicenseManager : MonoBehaviour
{
    private string filePath;

    public GameObject EndCanvas; // 라이선스 만료 시 표시할 UI 캔버스

    void Start()
    {
        // 안드로이드 포함 모든 플랫폼에서 안전한 저장 경로 설정
        filePath = Path.Combine(Application.persistentDataPath, ".sys_lic.dat");

        try
        {
            if (!File.Exists(filePath))
            {
                // 최초 실행일을 UTC 기준으로 ISO 8601 형식 저장 (예: 2025-08-20T13:45:00Z)
                string nowUtc = DateTime.UtcNow.ToString("o"); // 'o'는 ISO 8601 포맷
                File.WriteAllText(filePath, nowUtc);
                Debug.Log("최초 실행일 저장 완료");

            }
            else
            {
                // 저장된 날짜 읽기
                string savedDateStr = File.ReadAllText(filePath);

                // ISO 형식으로 파싱
                DateTime savedDate = DateTime.Parse(savedDateStr, null, System.Globalization.DateTimeStyles.RoundtripKind);

                /*double daysElapsed = (DateTime.UtcNow - savedDate).TotalDays;

                if (daysElapsed > 365)
                {
                    Debug.Log("사용 기간이 만료되었습니다!");
                    EndCanvas.transform.GetComponentInChildren<Button>().onClick.AddListener(() => Application.Quit()); // 버튼 클릭 시 앱 종료
                    EndCanvas.SetActive(true); // 라이선스 만료 UI 활성화
                }
                else
                {
                    int daysLeft = 365 - (int)daysElapsed;
                    Debug.Log("dat 파일 내용: " + savedDateStr);
                    Debug.Log($"남은 사용일: {daysLeft}일");
                }*/

                // 변경: '일'이 아니라 '시간' 단위로 경과 시간 계산
                TimeSpan elapsed = DateTime.UtcNow - savedDate;
                double hoursElapsed = elapsed.TotalHours;
                if (hoursElapsed > 0.5)
                {
                    Debug.Log("사용 기간이 만료되었습니다! (기준: 최초 실행 후 1시간)");
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
                    TimeSpan remain = TimeSpan.FromHours(1) - elapsed;
                    
                    // UI에 남은 시간 출력 (TextMeshPro)
                    string remainText = $"남은 시간: {Mathf.Max(0, (int)remain.TotalMinutes)}분 {Mathf.Max(0, remain.Seconds)}초";
                    GameObject textObj = GameObject.Find("TextLeft");
                    if (textObj != null)
                    {
                        TMP_Text tmp = textObj.GetComponent<TMP_Text>();
                        if (tmp != null)
                        {
                            tmp.text = remainText;
                        }
                        else
                        {
                            Debug.LogWarning("TextLeft 오브젝트에 TMP_Text 컴포넌트가 없습니다!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("씬에 'TextLeft' 오브젝트를 찾을 수 없습니다!");
                    }

                    Debug.Log("dat 파일 내용: " + savedDateStr);
                    Debug.Log($"남은 시간: {Mathf.Max(0, (int)remain.TotalMinutes)}분 {(int)remain.Seconds}초");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"라이선스 파일 처리 중 오류 발생: {ex.Message}");
        }
    }
}
