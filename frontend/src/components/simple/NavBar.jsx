import React from 'react';

const NavBar = ({data}) => {
	const menu = data.map(({name, onClick}) => {
		return (
			<a key={name} className={"nav-links-item"} onClick={onClick}>
				{name.toLowerCase()}
			</a>
		)
	});

	return (
		<nav className="nav-links">
			{menu}
		</nav>
	);
};

export default NavBar;
