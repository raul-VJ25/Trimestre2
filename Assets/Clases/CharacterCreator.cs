using UnityEngine;
using UnityEngine.UIElements;

public enum ATTRIBUTE
{
    FUERZA, RESISTENCIA, DESTREZA, INTELIGENCIA
}

public class CharacterCreator : MonoBehaviour
{
    CharacterData baseCharacterData;

    public UIDocument UI;
    private VisualElement root;

    private void Start()
    {
        baseCharacterData = nuevoPersonaje();
        root = UI.rootVisualElement;
        ActualizarUI();
    }

    private CharacterData nuevoPersonaje()
    {
        CharacterData nuevo = new CharacterData();
        nuevo.puntosDisponibles = 10;
        return nuevo;
    }

    public void asignarPunto(ATTRIBUTE atributo)
    {
        if (baseCharacterData.puntosDisponibles > 0)
        {
            baseCharacterData.puntosDisponibles--;

            switch (atributo)
            {
                case ATTRIBUTE.FUERZA:
                    baseCharacterData.fuerza += 1;
                    break;
                case ATTRIBUTE.RESISTENCIA:
                    baseCharacterData.resistencia += 1;
                    break;
                case ATTRIBUTE.DESTREZA:
                    baseCharacterData.destreza += 1;
                    break;
                case ATTRIBUTE.INTELIGENCIA:
                    baseCharacterData.inteligencia += 1;
                    break;
            }

        }

    }

    public void ActualizarUI()
    {
        TextField txtFuerza = root.Q<TextField>("txt_fuerza");
        txtFuerza.value = baseCharacterData.fuerza.ToString();
    }

}
