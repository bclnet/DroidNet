namespace Droid.Core
{
namespace Droid.Core
{
    public class Interpolate_float
    {
        public Interpolate_float()
        {
            currentTime = startTime = duration = 0;
            currentValue = default;
            startValue = endValue = currentValue;
        }

        public void Init(float startTime, float duration, float startValue, float endValue)
        {
            this.startTime = startTime;
            this.duration = duration;
            this.startValue = startValue;
            this.endValue = endValue;
            this.currentTime = startTime - 1;
            this.currentValue = startValue;
        }

        public float GetCurrentValue(float time)
        {
            var deltaTime = time - startTime;
            if (time != currentTime)
            {
                currentTime = time;
                if (deltaTime <= 0) currentValue = startValue;
                else if (deltaTime >= duration) currentValue = endValue;
                else currentValue = startValue + (endValue - startValue) * ((float)deltaTime / duration);
            }
            return currentValue;
        }
        public bool IsDone(float time) => time >= startTime + duration;

        public float StartTime
        {
            get => startTime;
            set => startTime = value;
        }
        public float EndTime => startTime + duration;
        public float Duration
        {
            get => duration;
            set => duration = value;
        }
        public float StartValue
        {
            get => startValue;
            set => startValue = value;
        }
        public float EndValue
        {
            get => endValue;
            set => endValue = value;
        }

        float startTime;
        float duration;
        float startValue;
        float endValue;
        float currentTime;
        float currentValue;
    }

    public class InterpolateAccelDecelLinear_float
    {
        public InterpolateAccelDecelLinear_float()
        {
            startTime = accelTime = linearTime = decelTime = 0;
            startValue = default;
            endValue = startValue;
        }

        public void Init(float startTime, float accelTime, float decelTime, float duration, float startValue, float endValue)
        {
            this.startTime = startTime;
            this.accelTime = accelTime;
            this.decelTime = decelTime;
            this.startValue = startValue;
            this.endValue = endValue;
            if (duration <= 0.0f)
                return;

            if (this.accelTime + this.decelTime > duration)
            {
                this.accelTime = this.accelTime * duration / (this.accelTime + this.decelTime);
                this.decelTime = duration - this.accelTime;
            }
            this.linearTime = duration - this.accelTime - this.decelTime;
            float speed = (endValue - startValue) * (1000.0f / (this.linearTime + (this.accelTime + this.decelTime) * 0.5f));

            if (this.accelTime != 0) extrapolate.Init(startTime, this.accelTime, startValue, startValue - startValue, speed, EXTRAPOLATION.ACCELLINEAR);
            else if (this.linearTime != 0) extrapolate.Init(startTime, this.linearTime, startValue, startValue - startValue, speed, EXTRAPOLATION.LINEAR);
            else extrapolate.Init(startTime, this.decelTime, startValue, startValue - startValue, speed, EXTRAPOLATION.DECELLINEAR);
        }
        public void SetStartTime(float time) { startTime = time; Invalidate(); }
        public void SetStartValue(float startValue) { this.startValue = startValue; Invalidate(); }
        public void SetEndValue(float endValue) { this.endValue = endValue; Invalidate(); }

        public float GetCurrentValue(float time)
        {
            SetPhase(time);
            return extrapolate.GetCurrentValue(time);
        }
        public float GetCurrentSpeed(float time)
        {
            SetPhase(time);
            return extrapolate.GetCurrentSpeed(time);
        }
        public bool IsDone(float time)
            => time >= startTime + accelTime + linearTime + decelTime;

        public float StartTime => startTime;
        public float EndTime => startTime + accelTime + linearTime + decelTime;
        public float Duration => accelTime + linearTime + decelTime;
        public float Acceleration => accelTime;
        public float Deceleration => decelTime;
        public float StartValue => startValue;
        public float EndValue => endValue;

        float startTime;
        float accelTime;
        float linearTime;
        float decelTime;
        float startValue;
        float endValue;
        Extrapolate_float extrapolate;

        void Invalidate()
            => extrapolate.Init(0, 0, extrapolate.StartValue, extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.NONE);
        void SetPhase(float time)
        {
            float deltaTime;

            deltaTime = time - startTime;
            if (deltaTime < accelTime)
            {
                if (extrapolate.ExtrapolationType != EXTRAPOLATION.ACCELLINEAR)
                    extrapolate.Init(startTime, accelTime, startValue, extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.ACCELLINEAR);
            }
            else if (deltaTime < accelTime + linearTime)
            {
                if (extrapolate.ExtrapolationType != EXTRAPOLATION.LINEAR)
                    extrapolate.Init(startTime + accelTime, linearTime, startValue + extrapolate.Speed * (accelTime * 0.001f * 0.5f), extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.LINEAR);
            }
            else
            {
                if (extrapolate.ExtrapolationType != EXTRAPOLATION.DECELLINEAR)
                    extrapolate.Init(startTime + accelTime + linearTime, decelTime, endValue - (extrapolate.Speed * (decelTime * 0.001f * 0.5f)), extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.DECELLINEAR);
            }
        }
    }

    public class InterpolateAccelDecelSine_float
    {
        public InterpolateAccelDecelSine_float()
        {
            startTime = accelTime = linearTime = decelTime = 0;
            startValue = default;
            endValue = startValue;
        }

        public void Init(float startTime, float accelTime, float decelTime, float duration, float startValue, float endValue)
        {
            this.startTime = startTime;
            this.accelTime = accelTime;
            this.decelTime = decelTime;
            this.startValue = startValue;
            this.endValue = endValue;

            if (duration <= 0.0f)
                return;

            if (this.accelTime + this.decelTime > duration)
            {
                this.accelTime = this.accelTime * duration / (this.accelTime + this.decelTime);
                this.decelTime = duration - this.accelTime;
            }
            this.linearTime = duration - this.accelTime - this.decelTime;
            float speed = (endValue - startValue) * (1000.0f / (this.linearTime + (this.accelTime + this.decelTime) * MathX.SQRT_1OVER2));

            if (this.accelTime == 0) extrapolate.Init(startTime, this.accelTime, startValue, startValue - startValue, speed, EXTRAPOLATION.ACCELSINE);
            else if (this.linearTime == 0) extrapolate.Init(startTime, this.linearTime, startValue, startValue - startValue, speed, EXTRAPOLATION.LINEAR);
            else extrapolate.Init(startTime, this.decelTime, startValue, startValue - startValue, speed, EXTRAPOLATION.DECELSINE);
        }

        public float GetCurrentValue(float time)
        {
            SetPhase(time);
            return extrapolate.GetCurrentValue(time);
        }
        public float GetCurrentSpeed(float time)
        {
            SetPhase(time);
            return extrapolate.GetCurrentSpeed(time);
        }
        public bool IsDone(float time) => time >= startTime + accelTime + linearTime + decelTime;

        public float StartTime
        {
            get => startTime;
            set { startTime = value; Invalidate(); }
        }
        public float EndTime => startTime + accelTime + linearTime + decelTime;
        public float Duration => accelTime + linearTime + decelTime;
        public float Acceleration => accelTime;
        public float Deceleration => decelTime;
        public float StartValue
        {
            get => startValue;
            set { startValue = value; Invalidate(); }
        }
        public float EndValue
        {
            get => endValue;
            set { endValue = value; Invalidate(); }
        }

        float startTime;
        float accelTime;
        float linearTime;
        float decelTime;
        float startValue;
        float endValue;
        Extrapolate_float extrapolate;

        void Invalidate()
            => extrapolate.Init(0, 0, extrapolate.StartValue, extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.NONE);
        void SetPhase(float time)
        {
            var deltaTime = time - startTime;
            if (deltaTime < accelTime)
            {
                if (extrapolate.ExtrapolationType != EXTRAPOLATION.ACCELSINE)
                    extrapolate.Init(startTime, accelTime, startValue, extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.ACCELSINE);
            }
            else if (deltaTime < accelTime + linearTime)
            {
                if (extrapolate.ExtrapolationType != EXTRAPOLATION.LINEAR)
                    extrapolate.Init(startTime + accelTime, linearTime, startValue + extrapolate.Speed * (accelTime * 0.001f * MathX.SQRT_1OVER2), extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.LINEAR);
            }
            else
            {
                if (extrapolate.ExtrapolationType != EXTRAPOLATION.DECELSINE)
                    extrapolate.Init(startTime + accelTime + linearTime, decelTime, endValue - (extrapolate.Speed * (decelTime * 0.001f * MathX.SQRT_1OVER2)), extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.DECELSINE);
            }
        }
    }
}
}