import {useEffect, useState} from 'react';
import { Link } from 'react-router-dom';
import './Leaderboard.css';
import logo from '../assets/logo.png';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export default function Leaderboard() {
    const [cats, setCats] = useState([]);
	const [loading, setLoading] = useState(false);

    useEffect(() => {
        setLoading(true);
        fetch(`${API_URL}/cats`)
            .then(response => response.json())
            .then(data => {
                setCats(data);
                setLoading(false);
            })
            .catch(error => {
                console.error('Error fetching cats:', error);
                setLoading(false);
            });
    }, []);

	if (loading) return <p>Chargement...</p> // Faire un vrai chargement plus tard
	if (cats.length === 0) return <p>Il n'y a aucune image de chat disponible.</p> // Faire une vraie page d'erreur plus tard

    return (
        <div className="leaderboard-page">
            <header className="site-header">
                <div className="logo">
                    <img src={logo} alt="Cat Icon" style={{ width: '40px', height: '40px' }} />
                    CATMASH
                </div>
            </header>

            <main className="leaderboard-main">
                <div className="top3">
                    <div className="top-item second">
                        {cats[1] && (
                            <div className="card">
                                <img src={cats[1].url} alt={`Chat ${cats[1].id}`} />
                                <div className="label">Chat {2}</div>
                                <div className="score">Score : {cats[1].score} pts</div>
                            </div>
                        )}
                    </div>

                    <div className="top-item first">
                        {cats[0] && (
                            <div className="card">
                                <img src={cats[0].url} alt={`Chat ${cats[0].id}`} />
                                <div className="label">Chat {1}</div>
                                <div className="score white">Score : {cats[0].score} pts</div>
                            </div>
                        )}
                    </div>

                    <div className="top-item third">
                        {cats[2] && (
                            <div className="card">
                                <img src={cats[2].url} alt={`Chat ${cats[2].id}`} />
                                <div className="label">Chat {3}</div>
                                <div className="score">Score : {cats[2].score} pts</div>
                            </div>
                        )}
                    </div>
                </div>

                <div className="grid">
                    {cats.slice(3).map((cat, idx) => (
                        <div className="grid-card" key={cat.id}>
                            <img src={cat.url} alt={`Chat ${cat.id}`} />
                            <div className="label">Chat {idx + 4}</div>
                            <div className="score">Score : {cat.score} pts</div>
                        </div>
                    ))}
                </div>
            </main>

            <footer className="site-footer">
                <div className="footer-inner">
                    <Link to="/vote"><button className="footer-button">Revenir au vote</button></Link>
                    <div className="matches-count">{cats.reduce((s,c)=>s+(c.score||0),0)} matchs joués</div>
                </div>
            </footer>
        </div>
    )
}