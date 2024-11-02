import FeedbackForm from '../components/simple/FeedbackForm.jsx';
import MapSection from '../components/simple/MapSection';
import Footer from '../components/Footer.jsx';
import Header from "../components/Header.jsx";
const Account = () => {

	return (
		<div className="container">
			<Header/>
			<FeedbackForm/>
			<MapSection/>
			<Footer/>
		</div>
	)
}

export default Account;