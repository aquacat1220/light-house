namespace LightHouse
{
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class AimLine : VisualElement
    {
        [SerializeField]
        float _innerAngle = 0f;
        [UxmlAttribute, CreateProperty]
        public float InnerAngle
        {
            get => _innerAngle;
            set
            {
                _innerAngle = value;
                RecalculateBound();
                MarkDirtyRepaint();
            }
        }

        [SerializeField]
        float _outerAngle = 0f;
        [UxmlAttribute, CreateProperty]
        public float OuterAngle
        {
            get => _outerAngle;
            set
            {
                _outerAngle = value;
                RecalculateBound();
                MarkDirtyRepaint();
            }
        }

        [SerializeField]
        float _lineLength = 0f;
        [UxmlAttribute, CreateProperty]
        public float LineLength
        {
            get => _lineLength;
            set
            {
                _lineLength = value;
                RecalculateBound();
                MarkDirtyRepaint();
            }
        }

        [SerializeField]
        float _lineWidth = 0f;
        [UxmlAttribute, CreateProperty]
        public float LineWidth
        {
            get => _lineWidth;
            set
            {
                _lineWidth = value;
                RecalculateBound();
                MarkDirtyRepaint();
            }
        }

        [SerializeField]
        float _stubLength = 0f;
        [UxmlAttribute, CreateProperty]
        public float StubLength
        {
            get => _stubLength;
            set
            {
                _stubLength = value;
                RecalculateBound();
                MarkDirtyRepaint();
            }
        }

        public AimLine()
        {
            // Add a custom visual content generator to the event.
            generateVisualContent += OnGenerateVisualContent;
            // We don't need to do cleanup; the event dies with the instance, so there is no risk of an event `Invoke()` triggering an invalid delegate.
        }

        void RecalculateBound()
        {
            this.style.height = LineLength + StubLength;
            this.style.width = 2 * (LineLength + StubLength) * Mathf.Sin(((InnerAngle / 2) + (OuterAngle / 2)) * Mathf.Deg2Rad);
        }

        void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            Painter2D painter = mgc.painter2D;
            painter.lineWidth = LineWidth;
            painter.lineCap = LineCap.Round;
            painter.strokeColor = Color.softRed;

            Vector2 start = new Vector2((LineLength + StubLength) * Mathf.Sin(((InnerAngle / 2) + (OuterAngle / 2)) * Mathf.Deg2Rad), 0f);

            painter.BeginPath();
            painter.MoveTo(start);
            painter.LineTo(start + Vector2.up * LineLength);



            Vector2 innerArcLeft = start + new Vector2(
                LineLength * Mathf.Cos((90f - (InnerAngle / 2)) * Mathf.Deg2Rad),
                LineLength * Mathf.Sin((90f - (InnerAngle / 2)) * Mathf.Deg2Rad)
            );
            Vector2 innerArcLeftShort = start + new Vector2(
                (LineLength - StubLength) * Mathf.Cos((90f - (InnerAngle / 2)) * Mathf.Deg2Rad),
                (LineLength - StubLength) * Mathf.Sin((90f - (InnerAngle / 2)) * Mathf.Deg2Rad)
            );

            Vector2 innerArcRight = start + new Vector2(
                LineLength * Mathf.Cos((90f + (InnerAngle / 2)) * Mathf.Deg2Rad),
                LineLength * Mathf.Sin((90f + (InnerAngle / 2)) * Mathf.Deg2Rad)
            );
            Vector2 innerArcRightShort = start + new Vector2(
                (LineLength - StubLength) * Mathf.Cos((90f + (InnerAngle / 2)) * Mathf.Deg2Rad),
                (LineLength - StubLength) * Mathf.Sin((90f + (InnerAngle / 2)) * Mathf.Deg2Rad)
            );

            painter.MoveTo(innerArcLeft);
            painter.Arc(start, LineLength, (90f - (InnerAngle / 2)), (90f - (InnerAngle / 2) - (OuterAngle / 2)), ArcDirection.CounterClockwise);
            painter.MoveTo(innerArcRight);
            painter.Arc(start, LineLength, (90f + (InnerAngle / 2)), (90f + (InnerAngle / 2) + (OuterAngle / 2)), ArcDirection.Clockwise);

            painter.MoveTo(innerArcLeft);
            painter.LineTo(innerArcLeftShort);
            painter.MoveTo(innerArcRight);
            painter.LineTo(innerArcRightShort);

            Vector2 outerArcLeftLong = start + new Vector2(
                (LineLength + StubLength) * Mathf.Cos((90f - (InnerAngle / 2) - (OuterAngle / 2)) * Mathf.Deg2Rad),
                (LineLength + StubLength) * Mathf.Sin((90f - (InnerAngle / 2) - (OuterAngle / 2)) * Mathf.Deg2Rad)
            );
            Vector2 outerArcLeftShort = start + new Vector2(
                (LineLength - StubLength) * Mathf.Cos((90f - (InnerAngle / 2) - (OuterAngle / 2)) * Mathf.Deg2Rad),
                (LineLength - StubLength) * Mathf.Sin((90f - (InnerAngle / 2) - (OuterAngle / 2)) * Mathf.Deg2Rad)
            );
            Vector2 outerArcRightLong = start + new Vector2(
                (LineLength + StubLength) * Mathf.Cos((90f + (InnerAngle / 2) + (OuterAngle / 2)) * Mathf.Deg2Rad),
                (LineLength + StubLength) * Mathf.Sin((90f + (InnerAngle / 2) + (OuterAngle / 2)) * Mathf.Deg2Rad)
            );
            Vector2 outerArcRightShort = start + new Vector2(
                (LineLength - StubLength) * Mathf.Cos((90f + (InnerAngle / 2) + (OuterAngle / 2)) * Mathf.Deg2Rad),
                (LineLength - StubLength) * Mathf.Sin((90f + (InnerAngle / 2) + (OuterAngle / 2)) * Mathf.Deg2Rad)
            );

            painter.MoveTo(outerArcLeftLong);
            painter.LineTo(outerArcLeftShort);
            painter.MoveTo(outerArcRightLong);
            painter.LineTo(outerArcRightShort);

            painter.Stroke();
        }
    }
}