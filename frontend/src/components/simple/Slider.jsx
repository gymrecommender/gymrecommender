import Input from "./Input.jsx";

const Slider = (props) => {
	return (
		<Input {...props}>
			<div className={'input-field-slider-range'}>
				<span className={"slider-min"}>{props.minText ?? props.min}</span>
				<span className={"slider-current"}>
					{props.isSplit ? `${props.max - props.value} / ${props.value}` : props.value }
				</span>
				<span className={"slider-max"}>{props.maxText ?? props.max}</span>
			</div>
		</Input>
	)
}

export default Slider;