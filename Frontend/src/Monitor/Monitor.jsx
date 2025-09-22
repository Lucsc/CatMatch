import React, { useEffect, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { Link } from "react-router-dom";
import logo from '../assets/logo.png';
import './Monitor.css';

const API_URL = import.meta.env.VITE_API_URL?.replace(/\/api\/?$/, "") || window.location.origin;

export default function Monitor() {
    const [connection, setConnection] = useState(null);
    const [events, setEvents] = useState([]);
    const [cats, setCats] = useState({}); // map by id: { id, url, score, updatedAt, updatedFlag }
    const [eventsCounts, setEventsCount] = useState(0);

    useEffect(() => {
        const hubUrl = `${API_URL}/hubs/monitor`;
        const conn = new HubConnectionBuilder()
            .withUrl(hubUrl, {
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        conn.start();

        conn.on("MonitoringUpdate", (payload) => {
            const id = payload?.Id ?? payload?.id ?? payload?.catId ?? payload?.CatId;
            const url = payload?.Url ?? payload?.url ?? payload?.ImageUrl ?? payload?.imageUrl ?? '';
            const newScore = (payload?.NewScore ?? payload?.newScore ?? payload?.score ?? payload?.Score);
            const tsRaw = payload?.TimeStamp ?? payload?.timeStamp ?? payload?.timestamp ?? new Date().toISOString();
            const ts = new Date(tsRaw).toISOString();

            setEvents(prev => [{ id: crypto?.randomUUID?.() ?? Math.random().toString(36).slice(2), payload, ts }, ...prev].slice(0, 200));

            if (id != null) {
                setCats(prev => {
                    const copy = { ...prev };
                    const existing = copy[id];
                    const isScoreChanged = existing && existing.score !== newScore;
                    copy[id] = {
                        id,
                        url: url || existing?.url || '',
                        score: newScore,
                        updatedAt: ts,
                        updatedFlag: true
                    };
                    setTimeout(() => {
                        setCats(current => {
                            const c = { ...current };
                            if (c[id]) c[id] = { ...c[id], updatedFlag: false };
                            return c;
                        });
                    }, 900);
                    return copy;
                });
            }

            setEventsCount(prev => prev + 1);
        });

        setConnection(conn);

        return () => {
            if (conn) conn.stop();
        };
    }, []);

    const catsArray = Object.values(cats).sort((a, b) => {
        if (a.updatedAt && b.updatedAt) return b.updatedAt.localeCompare(a.updatedAt);
        return 0;
    });

    return (
        <div className="monitor-page">
           <header className="site-header">
                <div className="logo">
                    <img src={logo} alt="Cat Icon" style={{ width: '40px', height: '40px' }} />
                   CATMASH
                </div>
           </header>

            <main className="monitor-main">
                <section className="monitor-center">
                    <h2>Monitoring en temps réel</h2>
                    <div className="connection-row">
                        <strong>SignalR:</strong> {connection ? "connected" : "disconnected"}
                    </div>

                    <div className="events-and-tabs">
                        <div className="events-panel">
                            <h3>Événements récents</h3>
                            <ul className="events-list">
                                {events.map(e => (
                                    <li key={e.id} className="event-item">
                                        <div className="event-ts">[{new Date(e.ts).toLocaleTimeString()}]</div>
                                        <div className="event-body">
                                            <code>{e.payload?.Type ?? e.payload?.type ?? 'Update'}</code>
                                            <div className="event-details">
                                                Id: {e.payload?.Id ?? e.payload?.id ?? '-'} —
                                                Score: {e.payload?.NewScore ?? e.payload?.newScore ?? e.payload?.score ?? '-'} —
                                                Url: {e.payload?.Url ?? e.payload?.url ?? '-'}
                                            </div>
                                        </div>
                                    </li>
                                ))}
                                {events.length === 0 && <li className="empty">Aucun évènement reçu.</li>}
                            </ul>
                        </div>

                        <div className="tabs-panel">
                            <h3>Chats votés (En direct)</h3>
                            <div className="cat-tabs" role="list">
                                {catsArray.length === 0 && <div className="empty">Aucune chat reçue</div>}
                                {catsArray.map(cat => (
                                    <div
                                        key={cat.id}
                                        role="listitem"
                                        className={`cat-tab ${cat.updatedFlag ? 'updated' : ''}`}
                                        title={`Chat ${cat.id} — ${cat.score ?? '-' } pts`}
                                    >
                                        <div className="tab-image-wrap">
                                            {cat.url ? <img src={cat.url} alt={`Chat ${cat.id}`} className="tab-image" /> : <div className="tab-image placeholder" />}
                                        </div>
                                        <div className="tab-score"> {cat.score ?? '-'} pts</div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>
                </section>
            </main>

            <footer className="site-footer">
                <div className="footer-inner">
                    <button className="footer-button" onClick={() => window.location.href = '/'}>Voir le classement des chats</button>
                    <div className="matches-count">{eventsCounts} événements reçus</div>
                </div>
            </footer>
        </div>
    );
}