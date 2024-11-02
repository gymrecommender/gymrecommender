import React from 'react';
import Logo from '../components/simple/Logo';
import Title from '../components/simple/Title';
import AuthButton from '../components/simple/AuthButton';
import InfoSection from '../components/simple/InfoSection';
import Sliders from '../components/simple/Sliders';
import LocationControls from '../components/simple/LocationControls';
import MapSection from '../components/simple/MapSection';
import Footer from '../components/simple/Footer';
import '../styles.css';
import NavBar from '../components/simple/NavBar';

const Index = () => {
	return (
		<div className="container">
			<Logo />
			<Title/>
			<NavBar/>
			<AuthButton />
			<InfoSection />
			<Sliders />
			<LocationControls />
			<MapSection />
			<Footer />
    	</div>
	)
}

export default Index;