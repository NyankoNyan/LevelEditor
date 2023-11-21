using System;
using UnityEngine;

namespace LevelView
{
    /// <summary>
    /// Настройки визуала для объектов уровня.
    /// </summary>
    /// <remarks>
    /// Эта настройка используется для протягивания объекта во все интерфейсы редактирования уровня.
    /// Её можно расширять для добавления новых свойств объектов.
    /// </remarks>
    [Serializable]
    public class ObjectSetup : IEquatable<ObjectSetup>
    {
        /// <summary>
        /// Уникальный идентификатор типа объекта. Используется как ключ. Одновременно не могут существовать два типа
        /// объекта с одним уникальным идентификатором.
        /// </summary>
        public string id;

        /// <summary>
        /// Ссылочный идентификатор. Используется в настройках уровня. Может быть неуникальным.
        /// Среди объектов с одинаковым refId будет использован наиболее подходящий в зависимости от
        /// настроек клиента.
        /// </summary>
        /// <remarks>
        /// Можно использовать разные объекты с одинаковым ссылочным идентифкатором для уровней LOD,
        /// клиентских девайсов разной производительности и создания специальных графических эффектов.
        /// </remarks>
        public string refId;

        /// <summary>
        /// Номер версии объекта. Нужен для версионирования настроек и разрешения кофликтов
        /// уникальных идентификаторов. Из двух объектов с одним id будет использован тот, чья
        /// версия больше.
        /// </summary>
        public uint version;

        /// <summary>
        /// Используемый префаб. Если префаб отсутствует, будет создан пустой объект, к которому
        /// будут дополнительно применены другие параметры, вроде меша, материала и текстуры. Если в
        /// дополнение к префабу заданы другие параметры, они заменят аналогичные параметры префаба.
        /// </summary>
        public GameObject prefab;

        public Mesh mesh;

        /// <summary>
        /// Основной материал, используемый объектом.
        /// </summary>
        /// <remarks>Дополнительные материалы пока не поддерживаются. Используйте префабы.</remarks>
        public Material material;

        /// <summary>
        /// Текстура. Для применения к материалу, у шейдера материала должен быть параметр mainTexture.
        /// </summary>
        /// <remarks>
        /// Дополнительные текстуры для нормалей и высот не поддерживаются. Используйте материалы.
        /// </remarks>
        public Texture mainTexture;

        public Vector3 offset;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;

        /// <summary>
        /// Помещает объект с применёнными offset, rotation и scale внутрь пустого объекта с
        /// Transform по умолчанию. Включение этого параметра может как упростить, так и усложнить
        /// жизнь при скриптинге, а также дать побочные эффекты, поэтому он опционален.
        /// </summary>
        public bool useTransformWrapper;

        /// <summary>
        /// Смещение для превью.
        /// </summary>
        /// <remarks>
        /// Вид объекта для превью используется в меню редактора уровней, для рендера иконок и
        /// прочего, что требует приведения разных объектов к единому размеру. Постарайтесь, чтобы
        /// объект влез в куб со стороной 1 метр.
        /// </remarks>
        public Vector3 previewOffset;

        /// <summary>
        /// Поворот для превью.
        /// </summary>
        /// <remarks>
        /// Вид объекта для превью используется в меню редактора уровней, для рендера иконок и
        /// прочего, что требует приведения разных объектов к единому размеру. Постарайтесь, чтобы
        /// объект влез в куб со стороной 1 метр.
        /// </remarks>
        public Vector3 previewRotation;

        /// <summary>
        /// Масштаб для превью.
        /// </summary>
        /// <remarks>
        /// Вид объекта для превью используется в меню редактора уровней, для рендера иконок и
        /// прочего, что требует приведения разных объектов к единому размеру. Рекомендуется, чтобы
        /// объект влезал в куб со стороной 1 метр.
        /// </remarks>
        public Vector3 previewScale = Vector3.one;

        public bool Equals(ObjectSetup other)
        {
            return id == other.id && refId == other.refId
                && prefab == other.prefab && mesh == other.mesh && mesh.mainTexture == other.mainTexture
                && offset == other.offset && rotation == other.rotation && scale == other.scale;
        }

    }

    /// <summary>
    /// Список отображаемых объектов сцены
    /// </summary>
    [CreateAssetMenu( fileName = "ObjectSetupListManifest", menuName = "LevelEditor/ObjectSetupListManifest" )]
    public class ObjectSetupListManifest : ScriptableObject
    {
        [SerializeField] public ObjectSetup[] Objects;
    }
}