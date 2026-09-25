using UnityEngine;

public class TentacleController : MonoBehaviour
{
    [Header("Режимы")]
    [Tooltip("Включить обратную кинематику (IK). Если выключено - работает прямая (FK).")]
    public bool useIK = false;

    [Header("Ссылки на объекты")]
    public Transform joint1;
    public Transform joint2;
    public Transform joint3;
    public Transform joint4;

    public Transform endEffector;
    public Transform target;

    [Header("Углы суставов (в градусах)")]
    [Range(-85f, 85f)] public float angle1 = 0f;
    [Range(-85f, 85f)] public float angle2 = 0f;
    [Range(-85f, 85f)] public float angle3 = 0f;
    [Range(-85f, 85f)] public float angle4 = 0f;

    [Header("Настройки IK")]
    [Tooltip("Скорость реакции щупальца в режиме IK")]
    public float ikSpeed = 5f;

    private float maxBend = 85f;

    void Update()
    {
        if (!useIK)
        {
            // ==========================================
            // РЕЖИМ 1: ПРЯМАЯ КИНЕМАТИКА (FK)
            // ==========================================

            // 1. Ограничиваем углы (на случай, если значение пришло извне)
            angle1 = Mathf.Clamp(angle1, -maxBend, maxBend);
            angle2 = Mathf.Clamp(angle2, -maxBend, maxBend);
            angle3 = Mathf.Clamp(angle3, -maxBend, maxBend);
            angle4 = Mathf.Clamp(angle4, -maxBend, maxBend);

            // 2. Применяем вращение по слайдерам
            joint1.localRotation = Quaternion.Euler(0, 0, angle1);
            joint2.localRotation = Quaternion.Euler(0, 0, angle2);
            joint3.localRotation = Quaternion.Euler(0, 0, angle3);
            joint4.localRotation = Quaternion.Euler(0, 0, angle4);

            // 3. Target бегает за кончиком щупальца (условие Лаб 1)
            if (target != null && endEffector != null)
            {
                target.position = endEffector.position;
            }
        }
        else
        {
            // ==========================================
            // РЕЖИМ 2: ОБРАТНАЯ КИНЕМАТИКА (IK)
            // Метод Якоби (Jacobian Transpose) для доп. баллов!
            // ==========================================

            Vector2 targetPos = target.position;
            Vector2 endPos = endEffector.position;
            Vector2 error = targetPos - endPos;

            // Если уже достигли цели (погрешность меньше 1 см), не дергаемся
            if (error.magnitude < 0.01f) return;

            // Векторы от каждого сустава к концу щупальца (в мировых координатах)
            Vector2 r1 = endPos - (Vector2)joint1.position;
            Vector2 r2 = endPos - (Vector2)joint2.position;
            Vector2 r3 = endPos - (Vector2)joint3.position;
            Vector2 r4 = endPos - (Vector2)joint4.position;

            // Вычисляем градиент для каждого угла.
            // В 2D это Z-компонента векторного произведения (R x Error).
            // Это и есть строка транспонированной матрицы Якоби, умноженная на вектор ошибки.
            float dTheta1 = (r1.x * error.y - r1.y * error.x) * ikSpeed * Time.deltaTime;
            float dTheta2 = (r2.x * error.y - r2.y * error.x) * ikSpeed * Time.deltaTime;
            float dTheta3 = (r3.x * error.y - r3.y * error.x) * ikSpeed * Time.deltaTime;
            float dTheta4 = (r4.x * error.y - r4.y * error.x) * ikSpeed * Time.deltaTime;

            // Обновляем углы и СРАЗУ применяем ограничения, чтобы исключить коллизии сегментов
            angle1 = Mathf.Clamp(angle1 + dTheta1, -maxBend, maxBend);
            angle2 = Mathf.Clamp(angle2 + dTheta2, -maxBend, maxBend);
            angle3 = Mathf.Clamp(angle3 + dTheta3, -maxBend, maxBend);
            angle4 = Mathf.Clamp(angle4 + dTheta4, -maxBend, maxBend);

            // Применяем новые вычисленные углы к трансформам
            joint1.localRotation = Quaternion.Euler(0, 0, angle1);
            joint2.localRotation = Quaternion.Euler(0, 0, angle2);
            joint3.localRotation = Quaternion.Euler(0, 0, angle3);
            joint4.localRotation = Quaternion.Euler(0, 0, angle4);
        }
    }
}