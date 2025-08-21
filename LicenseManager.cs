using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LicenseManager : MonoBehaviour
{
    private string filePath;

    public GameObject EndCanvas; // ���̼��� ���� �� ǥ���� UI ĵ����

    void Start()
    {
        // �ȵ���̵� ���� ��� �÷������� ������ ���� ��� ����
        filePath = Path.Combine(Application.persistentDataPath, ".sys_lic.dat");

        try
        {
            if (!File.Exists(filePath))
            {
                // ���� �������� UTC �������� ISO 8601 ���� ���� (��: 2025-08-20T13:45:00Z)
                string nowUtc = DateTime.UtcNow.ToString("o"); // 'o'�� ISO 8601 ����
                File.WriteAllText(filePath, nowUtc);
                Debug.Log("���� ������ ���� �Ϸ�");

            }
            else
            {
                // ����� ��¥ �б�
                string savedDateStr = File.ReadAllText(filePath);

                // ISO �������� �Ľ�
                DateTime savedDate = DateTime.Parse(savedDateStr, null, System.Globalization.DateTimeStyles.RoundtripKind);

                /*double daysElapsed = (DateTime.UtcNow - savedDate).TotalDays;

                if (daysElapsed > 365)
                {
                    Debug.Log("��� �Ⱓ�� ����Ǿ����ϴ�!");
                    EndCanvas.transform.GetComponentInChildren<Button>().onClick.AddListener(() => Application.Quit()); // ��ư Ŭ�� �� �� ����
                    EndCanvas.SetActive(true); // ���̼��� ���� UI Ȱ��ȭ
                }
                else
                {
                    int daysLeft = 365 - (int)daysElapsed;
                    Debug.Log("dat ���� ����: " + savedDateStr);
                    Debug.Log($"���� �����: {daysLeft}��");
                }*/

                // ����: '��'�� �ƴ϶� '�ð�' ������ ��� �ð� ���
                TimeSpan elapsed = DateTime.UtcNow - savedDate;
                double hoursElapsed = elapsed.TotalHours;
                if (hoursElapsed > 0.5)
                {
                    Debug.Log("��� �Ⱓ�� ����Ǿ����ϴ�! (����: ���� ���� �� 1�ð�)");
                    if (EndCanvas != null)
                    {
                        Button quitBtn = EndCanvas.transform.GetComponentInChildren<Button>();
                        if (quitBtn != null)
                            quitBtn.onClick.AddListener(() => Application.Quit()); // ��ư Ŭ�� �� �� ����
                        EndCanvas.SetActive(true); // ���̼��� ���� UI Ȱ��ȭ
                    }
                }
                else
                {
                    TimeSpan remain = TimeSpan.FromHours(1) - elapsed;
                    
                    // UI�� ���� �ð� ��� (TextMeshPro)
                    string remainText = $"���� �ð�: {Mathf.Max(0, (int)remain.TotalMinutes)}�� {Mathf.Max(0, remain.Seconds)}��";
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
                            Debug.LogWarning("TextLeft ������Ʈ�� TMP_Text ������Ʈ�� �����ϴ�!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("���� 'TextLeft' ������Ʈ�� ã�� �� �����ϴ�!");
                    }

                    Debug.Log("dat ���� ����: " + savedDateStr);
                    Debug.Log($"���� �ð�: {Mathf.Max(0, (int)remain.TotalMinutes)}�� {(int)remain.Seconds}��");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"���̼��� ���� ó�� �� ���� �߻�: {ex.Message}");
        }
    }
}
