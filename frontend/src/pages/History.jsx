import BodyHistory from '../components/simple/BodyHistory.jsx';
import Footer from '../components/Footer.jsx';
import Header from "../components/Header.jsx";
import '../styles_history.css'

const History = () => {
	return (
		<div className="container">
			<Header/>
			<BodyHistory/>
			<Footer/>
		</div>
	)
}

export default History;