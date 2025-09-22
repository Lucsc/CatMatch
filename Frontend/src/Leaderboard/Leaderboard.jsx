import {useEffect, useState} from 'react';
import { Link } from 'react-router-dom';
import './Leaderboard.css';
import logo from '../assets/logo.png';
import ErrorPage from '../utils/ErrorPage';
import LoadingPage from '../utils/LoadingPage';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export default function Leaderboard() {
    const [cats, setCats] = useState([]);
    const [loading, setLoading] = useState(false);
    const [period, setPeriod] = useState("all");

    useEffect(() => {
        setLoading(true);
        fetch(`${API_URL}/cats?period=${period}`)
            .then(response => response.json())
            .then(data => {
                setCats(data);
                setLoading(false);
            })
            .catch(error => {
                console.error('Error fetching cats:', error);
                setLoading(false);
            });
    }, [period]);

    const periods = [
        { value: "all", label: 'Tous les temps' },
        { value : 'last hour', label: 'Cette heure' },
        { value: 'last day', label: 'Aujourd\'hui' },
        { value: 'last week', label: 'Cette semaine' },
        { value: 'last month', label: 'Ce mois-ci' },
        { value: 'last year', label: 'Cette année' }
    ];

    let content;

    if (loading) {
        content = (
            <div className="loading">
                <LoadingPage />
            </div>
        );
    } else if (cats.length === 0) {
        content = <ErrorPage message="Aucun chat trouvé pour cette période." />;
    } else {
        content = (
            <>
                <div className="top3">
                    <div className="top-item second">
                        {cats[1] && (
                            <div className="card top-card">
                                <img src={cats[1].url} alt={`Chat ${cats[1].id}`} />
                                <div className="label">#2</div>
                                <div className="score">Score : {cats[1].score} pts</div>
                            </div>
                        )}
                    </div>
                    <div className="top-item first">
                        {cats[0] && (
                            <div className="card top-card first-card">
                                <img src={cats[0].url} alt={`Chat ${cats[0].id}`} />
                                <div className="label">#1</div>
                                <div className="score white">Score : {cats[0].score} pts</div>
                            </div>
                        )}
                    </div>
                    <div className="top-item third">
                        {cats[2] && (
                            <div className="card top-card">
                                <img src={cats[2].url} alt={`Chat ${cats[2].id}`} />
                                <div className="label">#3</div>
                                <div className="score">Score : {cats[2].score} pts</div>
                            </div>
                        )}
                    </div>
                </div>
                <div className="grid">
                    {cats.slice(3).map((cat, idx) => (
                        <div className="grid-card card" key={cat.id}>
                            <img src={cat.url} alt={`Chat ${cat.id}`} />
                            <div className="label">#{idx + 4}</div>
                            <div className="score">Score : {cat.score} pts</div>
                        </div>
                    ))}
                </div>
            </>
        );
    }

    return (
        <div className="leaderboard-page">
            <header className="site-header">
                <div className="logo">
                    <img src={logo} alt="Cat Icon" style={{ width: '40px', height: '40px' }} />
                    <span className="logo-text">CATMASH</span>
                </div>
                <Link to="/monitor" className="monitor-button">
                    <button>Voir les votes en temps réel</button>
                </Link>
            </header>

            <main className="leaderboard-main">
                <div className="leaderboard-filter">
                    <label htmlFor="period-select">Filtrer par période :</label>
                    <select
                        id="period-select"
                        value={period}
                        onChange={e => setPeriod(e.target.value)}
                        className="period-select"
                    >
                        {periods.map(p => (
                            <option key={p.value} value={p.value}>{p.label}</option>
                        ))}
                    </select>
                </div>
                {content}
            </main>

            <footer className="site-footer">
                <div className="footer-inner">
                    <Link to="/vote">
                        <button className="footer-button">Revenir au vote</button>
                    </Link>
                    <div className="matches-count">
                        {cats.reduce((s, c) => s + (c.score || 0), 0)} matchs joués
                    </div>
                </div>
            </footer>
        </div>
    );
}