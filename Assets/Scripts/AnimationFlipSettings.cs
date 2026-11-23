using UnityEngine;

// Guarda qué animaciones del Player hay que voltear horizontalmente.
// Es un asset (no un componente), así que los cambios hechos en Play Mode
// se conservan al salir de Play.
[CreateAssetMenu(fileName = "PlayerAnimationFlip", menuName = "Player/Animation Flip Settings")]
public class AnimationFlipSettings : ScriptableObject
{
    [System.Serializable]
    public class AnimationFlip
    {
        public string stateName;
        public bool flip;

        // Unity necesita un constructor vacío para poder cargar el asset
        public AnimationFlip() { }

        public AnimationFlip(string stateName)
        {
            this.stateName = stateName;
        }
    }

    // Un elemento por cada estado del Animator que usa PlayerMovement
    public AnimationFlip[] animations =
    {
        new AnimationFlip("Player_Idle"),
        new AnimationFlip("Player_LongIdle"),
        new AnimationFlip("Player_Walk"),
        new AnimationFlip("Player_Run"),
        new AnimationFlip("Player_Jump"),
        new AnimationFlip("Player_Fall"),
        new AnimationFlip("Player_Attack"),
    };

    public bool IsFlipped(string stateName)
    {
        foreach (AnimationFlip animation in animations)
        {
            if (animation.stateName == stateName)
            {
                return animation.flip;
            }
        }
        return false;
    }
}
