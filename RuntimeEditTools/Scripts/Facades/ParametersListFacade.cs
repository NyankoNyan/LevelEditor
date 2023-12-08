using Level;
using LevelView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    public interface IParamElement
    {
        string Name { set; }
        object Value { set; }
        bool ReadOnly { set; }
        bool Selected { set; }
    }

    internal abstract class EditConnector
    {
        public abstract object Value { get; set; }
    }

    internal class StringEditConnector : EditConnector
    {
        private IParametersConnector _parametersConnector;
        private string _paramName;

        public StringEditConnector(IParametersConnector parametersConnector, string paramName)
        {
            _parametersConnector = parametersConnector;
            _paramName = paramName;
        }

        public override object Value
        {
            get => _parametersConnector.GetParameter( _paramName );
            set => _parametersConnector.SetParameter( _paramName, value );
        }
    }

    internal class Vector3IntEditConnector : EditConnector
    {
        private IParametersConnector _parametersConnector;
        private string _paramName;
        private int _vecPart;

        public Vector3IntEditConnector(IParametersConnector parametersConnector, string paramName, int vecPart)
        {
            _parametersConnector = parametersConnector;
            _paramName = paramName;
            _vecPart = vecPart;
        }

        public override object Value
        {
            get => ( (Vector3Int)_parametersConnector.GetParameter( _paramName ) )[_vecPart].ToString();
            set {
                var vec = (Vector3Int)_parametersConnector.GetParameter( _paramName );
                string strVal = (string)value;
                if (string.IsNullOrWhiteSpace( strVal )) {
                    vec[_vecPart] = 0;
                } else {
                    vec[_vecPart] = int.Parse( strVal );
                }
                _parametersConnector.SetParameter( _paramName, vec );
            }
        }
    }

    internal class Vector3EditConnector : EditConnector
    {
        private IParametersConnector _parametersConnector;
        private string _paramName;
        private int _vecPart;

        public Vector3EditConnector(IParametersConnector parametersConnector, string paramName, int vecPart)
        {
            _parametersConnector = parametersConnector;
            _paramName = paramName;
            _vecPart = vecPart;
        }

        public override object Value
        {
            get => ( (Vector3)_parametersConnector.GetParameter( _paramName ) )[_vecPart].ToString( "0.00" );
            set {
                var vec = (Vector3)_parametersConnector.GetParameter( _paramName );
                string strVal = (string)value;
                if (string.IsNullOrWhiteSpace( strVal )) {
                    vec[_vecPart] = 0;
                } else {
                    vec[_vecPart] = float.Parse( strVal );
                }
                _parametersConnector.SetParameter( _paramName, vec );
            }
        }
    }

    internal class Bounds3DEditConnector : EditConnector
    {
        private IParametersConnector _parametersConnector;
        private string _paramName;
        private byte _subpart;

        public Bounds3DEditConnector(IParametersConnector parametersConnector, string paramName, byte subpart)
        {
            _parametersConnector = parametersConnector;
            _paramName = paramName;
            _subpart = subpart;
        }

        public override object Value
        {
            get {
                var bounds = (GridBoundsRect)_parametersConnector.GetParameter( _paramName );
                if (_subpart < 3) {
                    return bounds.chunkFrom[_subpart];
                } else {
                    return bounds.chunkTo[_subpart - 3];
                }
            }
            set {
                var bounds = (GridBoundsRect)_parametersConnector.GetParameter( _paramName );
                string strVal = (string)value;
                int intVal = 0;
                if (!string.IsNullOrWhiteSpace( strVal )) {
                    intVal = int.Parse( strVal );
                }
                if (_subpart < 3) {
                    bounds.chunkFrom[_subpart] = intVal;
                } else {
                    bounds.chunkTo[_subpart - 3] = intVal;
                }
                _parametersConnector.SetParameter( _paramName, bounds );
            }
        }
    }

    public class ParametersListFacade : MonoBehaviour
    {
        [SerializeField] private Transform _elementsRoot;
        [SerializeField] private BoolParamElement _boolPrefab;
        [SerializeField] private StringParamElement _stringPrefab;
        [SerializeField] private V3ParamElement _vector3Prefab;
        [SerializeField] private V3ParamElement _vector3IntPrefab;
        [SerializeField] private Bounds3DParamElement _bounds3DPrefab;
        [SerializeField] private UnityEngine.UI.Button _backButton;

        private IParametersConnector _parametersConnector;
        private Dictionary<string, IParamElement> _elements = new();
        private string _focusedElementId;

        internal Action<string, EditConnector> OnParameterFocused { get; set; }
        internal Action<string> OnParameterUnfocused { get; set; }

        public Action OnBack { get; set; }

        public Dictionary<string, IParamElement> Elements => _elements;

        public void Init(IParametersConnector parametersConnector)
        {
            _parametersConnector = parametersConnector;
            CreateListElements();
            _parametersConnector.OnParamModelUpdate += OnParamModelUpdate;
            _backButton.onClick.AddListener( () => {
                OnBack?.Invoke();
            } );
        }

        private void OnParamModelUpdate(string name, object value)
        {
            var elem = _elements[name];
            if (value is uint intval) {
                elem.Value = intval.ToString();
            } else {
                elem.Value = value;
            }
        }

        private void CreateListElements()
        {
            foreach (ParameterDescription descr in _parametersConnector.ParametersDescriptions) {
                IParamElement elementProto;
                if (descr.type == typeof( bool )) {
                    elementProto = Instantiate( _boolPrefab, _elementsRoot );
                } else if (descr.type == typeof( string )) {
                    elementProto = Instantiate( _stringPrefab, _elementsRoot );
                } else if (descr.type == typeof( uint )) {
                    elementProto = Instantiate( _stringPrefab, _elementsRoot );
                } else if (descr.type == typeof( Vector3Int )) {
                    elementProto = Instantiate( _vector3IntPrefab, _elementsRoot );
                } else if (descr.type == typeof( Vector3 )) {
                    elementProto = Instantiate( _vector3Prefab, _elementsRoot );
                } else if (descr.type == typeof( GridBoundsRect )) {
                    elementProto = Instantiate( _bounds3DPrefab, _elementsRoot );
                } else {
                    Debug.LogError( $"Unknown type {descr.type.Name}" );
                    return;
                }
                elementProto.Name = descr.name;
                elementProto.ReadOnly = descr.mode == ParameterMode.ReadOnly;
                _elements.Add( descr.name, elementProto );

                if (descr.mode == ParameterMode.ReadWrite) {
                    if (descr.type == typeof( Vector3Int )
                        || descr.type == typeof( Vector3 )
                        || descr.type == typeof( GridBoundsRect )) {
                        ( elementProto as V3ParamElement ).OnComponentSelect += (subpart) => {
                            if (_focusedElementId != descr.name) {
                                Unfocuse();
                            }

                            EditConnector connector;
                            if (descr.type == typeof( Vector3Int )) {
                                connector = new Vector3IntEditConnector( _parametersConnector, descr.name, subpart );
                            } else if (descr.type == typeof( Vector3 )) {
                                connector = new Vector3EditConnector( _parametersConnector, descr.name, subpart );
                            } else {
                                connector = new Bounds3DEditConnector( _parametersConnector, descr.name, (byte)subpart );
                            }

                            OnParameterFocused?.Invoke( descr.name, connector );
                            _focusedElementId = descr.name;
                        };
                    }

                    if (descr.type == typeof( string )) {
                        var listElement = ( elementProto as MonoBehaviour ).GetComponent<ListUIElement>();
                        listElement.onClick += (_) => {
                            Unfocuse();
                            listElement.Selected = !listElement.Selected;
                            var connector = new StringEditConnector( _parametersConnector, descr.name );
                            OnParameterFocused?.Invoke( descr.name, connector );
                            _focusedElementId = descr.name;
                        };
                    }
                }
            }
        }

        private void OnDestroy()
        {
            _parametersConnector.OnParamModelUpdate -= OnParamModelUpdate;
            _backButton.onClick.RemoveAllListeners();
        }

        private void Unfocuse()
        {
            if (_focusedElementId != null) {
                _elements[_focusedElementId].Selected = false;
                OnParameterUnfocused?.Invoke( _focusedElementId );
                _focusedElementId = null;
            }
        }
    }
}