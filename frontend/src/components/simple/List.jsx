import classNames from "classnames";

const List = ({data, className, id, renderItem}) => {
	const listItems = data.map(({item}) => {
		return renderItem(item)
	})

	return (
		<ul className={classNames(className)} id={id}>
			{listItems}
		</ul>
	)
}

export default List;