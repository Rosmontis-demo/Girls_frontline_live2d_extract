using Newtonsoft.Json.Linq;
using System;


namespace CubismFadeMotionDataToJson
{
    
    public class MotionDataConverter

    {
        public MotionDataConverter()
        {

        }
        public JObject Convert(JObject cubismFadeMotionData)
        {
            JObject result = new JObject();
            WriteHead(result);

            WriteCurves(result, (JObject)cubismFadeMotionData.GetValue("0 MonoBehaviour Base"));
            return result;
        }
        class Keyframe
        {
            public float time, value, inSlope, outSlope;
        }
        void WriteHead(JObject dstData)
        {
            dstData.Add("Version", 3);
            JObject meta = new JObject();
            meta.Add("Duration", 0.0f);
            meta.Add("Pps", 0.0f);
            meta.Add("Loop",true);
            meta.Add("AreBeziersRestricted",true);
            meta.Add("CurveCount", 0.0f);
            meta.Add("TotalSegmentCount", 0.0f);
            meta.Add("TotalPointCount", 0.0f);
            meta.Add("UserDataCount", 0.0f);
            meta.Add("TotalUserDataSize", 0.0f);
            dstData.Add("Meta", meta);
        }
        void WriteCurves(JObject dstData, JObject srcData)
        {
            JArray curves = new JArray();
            string[] parameterIds = GetParameterIds(srcData);

            float[] parameterFadeInTimes = GetParameterFadeInTimes(srcData);


            float[] parameterFadeOutTimes =  GetParameterFadeOutTimes(srcData);



            JArray animationCurves = (JArray)((JObject)srcData.GetValue("0 vector ParameterCurves")).GetValue("1 Array Array");
            
            for (int i = 0; i < parameterIds.Length; ++i)
            {
                if (string.IsNullOrEmpty(parameterIds[i]))
                    continue;
                JObject curve = new JObject();
                curve.Add("Target", "Parameter");
                curve.Add("Id", parameterIds[i]);
                if (parameterFadeInTimes[i] >= 0.0f)
                    curve.Add("FadeInTimes", parameterFadeInTimes[i]);
                if (parameterFadeOutTimes[i] >= 0.0f)
                    curve.Add("FadeOutTimes", parameterFadeOutTimes[i]);

                curve.Add("Segments", ConvertKeyFramesToCurvesSegments((JObject)((JObject)animationCurves[i]).GetValue("0 AnimationCurve data")) );
                curves.Add(curve);
                
            }
            dstData.Add("Curves", curves);
        }
        string[] GetParameterIds(JObject srcData)
        {
            JArray array = (JArray)((JObject)srcData.GetValue("0 vector ParameterIds")).GetValue("1 Array Array");
            string[] result = new string[array.Count];
            for (int i = 0; i < result.Length; ++i)
                result[i] = (string)((JObject)array[i]).GetValue("1 string data");
            return result;
        }
        
            
        float[] GetParameterFadeInTimes(JObject srcData)
        {
            JArray array = (JArray)((JObject)srcData.GetValue("0 vector ParameterFadeInTimes")).GetValue("1 Array Array");
            float[] result = new float[array.Count];
            for (int i = 0; i < result.Length; ++i)
                result[i] = (float)((JObject)array[i]).GetValue("0 float data");
            return result;
        }
        float[] GetParameterFadeOutTimes(JObject srcData)
        {
            JArray array = (JArray)((JObject)srcData.GetValue("0 vector ParameterFadeOutTimes")).GetValue("1 Array Array");
            float[] result = new float[array.Count];
            for (int i = 0; i < result.Length; ++i)
                result[i] = (float)((JObject)array[i]).GetValue("0 float data");
            return result;
        }
        
        JArray ConvertKeyFramesToCurvesSegments(JObject animationCurves)
        
        {
            
            JArray result = new JArray();

            JArray curveArray = (JArray)((JObject)animationCurves.GetValue("0 vector m_Curve")).GetValue("1 Array Array");
            if (curveArray.Count == 0)
                return result;

            Keyframe[] keyframe = ConvertJsonToArray(curveArray);
            //first 2 keyframe must be segment
            result.Add(keyframe[0].time);
            result.Add(keyframe[0].value);
            for (int j = 1; j < keyframe.Length; ++j)
            {
                //judge keyfraame type
                if (j + 1 < keyframe.Length && keyframe[j].inSlope != 0.0f && keyframe[j].outSlope == 0.0f && keyframe[j +1].inSlope == 0.0f &&keyframe[j+1].inSlope == 0.0f)
                {
                    result.Add(3.0f); //type:inverseStepped
                    result.Add(keyframe[j + 1].time);
                    result.Add(keyframe[j + 1].value);
                    ++j; //inversestepped create 2 keyframe
                }
                else if (float.IsPositiveInfinity(keyframe[j].inSlope))
                {
                    result.Add(2.0f);
                    result.Add(keyframe[j].time);
                    result.Add(keyframe[j].value);
                }
           
                
                else if (keyframe[j - 1].outSlope == keyframe[j].inSlope)
                {
                    result.Add(0.0f);
                    result.Add(keyframe[j].time);
                    result.Add(keyframe[j].value);
                }
                else
                {
                    result.Add(1.0f);
                    float tangentLength = Math.Abs(keyframe[j - 1].time - keyframe[j].time) * 0.333333f;
                    result.Add(0.0f);
                    
                    result.Add(keyframe[j - 1].outSlope * tangentLength + keyframe[j - 1].value);
                    result.Add(0.0f);
                    result.Add(keyframe[j].value - keyframe[j].inSlope * tangentLength);
                    result.Add(keyframe[j].time);
                    result.Add(keyframe[j].value);
                }
                    

            }
            return result;
        }
        Keyframe[] ConvertJsonToArray(JArray array)
        {
            Keyframe[] result = new Keyframe[array.Count];
            for (int i = 0; i < array.Count; ++i)
            {
                JObject obj = (JObject)((JObject)array[i]).GetValue("0 Keyframe data");
                result[i] = new Keyframe();
                result[i].time = (float)obj.GetValue("0 float time");
                result[i].value = (float)obj.GetValue("0 float value");
                var test = obj.GetValue("0 float inSlope");
                float max;
                if(float.TryParse(test.ToString(), out max))
                {
                    result[i].inSlope = (float)obj.GetValue("0 float inSlope");
                }
                else
                {
                    result[i].inSlope = float.MaxValue;
                }
                
                result[i].outSlope = (float)obj.GetValue("0 float outSlope");
            }
            return result;

        }
    }
}
