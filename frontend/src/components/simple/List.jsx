const List = ({data, className, id, renderItem}) => {
	const listItems = data.map(({item}) => {
		return renderItem(item)
	})

	return (
		<ul className={`${className} custom-class-name`} id={id}>
			{listItems}
		</ul>
	)
}

export default List;