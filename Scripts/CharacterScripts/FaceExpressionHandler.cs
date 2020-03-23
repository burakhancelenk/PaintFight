using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets ;
using UnityEngine;

public class FaceExpressionHandler : MonoBehaviour
{
    public enum FaceExpressions
    {
        ANGRY,SAD,SMILE,SUPRİSED
    }
    
    /* Eye expressions index meanings
     * 0 - Angry
     * 1 - Sad
     * 2 - Standart
     * 3 - Suprised
     *
     * Mouth expressions index meanings
     * 0 - Smile
     * 1 - Sad
     * 2 - Suprised
     */
    public FaceExpressions CurrentExpression
    {
        get { return currentExpression ; }
    }

    public Texture[] EyeExpressions ;
    public Texture[] MouthExpressions ;
    private FaceExpressions currentExpression ;

    private MeshRenderer characterRenderer ;
    

    private void Start()
    {
        characterRenderer = transform.Find("CharacterEquipments").Find("CharacterShape").GetComponent<MeshRenderer>() ;
        currentExpression = FaceExpressions.SMILE ;
    }
    
    public void ChangeFaceExpression(FaceExpressions fe)
    {
        switch (fe)
        {
            case FaceExpressions.ANGRY:
                characterRenderer.materials[1].mainTexture = EyeExpressions[0] ;
                characterRenderer.materials[2].mainTexture = MouthExpressions[0] ;
                currentExpression = FaceExpressions.ANGRY ;
                break ;
            case FaceExpressions.SAD:
                characterRenderer.materials[1].mainTexture = EyeExpressions[1] ;
                characterRenderer.materials[2].mainTexture = MouthExpressions[1] ;
                currentExpression = FaceExpressions.SAD ;
                break;
            case FaceExpressions.SMILE:
                characterRenderer.materials[1].mainTexture = EyeExpressions[2] ;
                characterRenderer.materials[2].mainTexture = MouthExpressions[0] ;
                currentExpression = FaceExpressions.SMILE ;
                break;
            case FaceExpressions.SUPRİSED:
                characterRenderer.materials[1].mainTexture = EyeExpressions[3] ;
                characterRenderer.materials[2].mainTexture = MouthExpressions[2] ;
                currentExpression = FaceExpressions.SUPRİSED ;
                break;
        }
    }
}
