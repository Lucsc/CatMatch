import { BrowserRouter, Link, Routes, Route } from 'react-router-dom'
import Leaderboard from './Leaderboard/Leaderboard'

function App() {
  return (
    <BrowserRouter>
      <header style={{marginBottom:20}}>
        <nav>
          <Link to="/">Leaderboard</Link>
        </nav>
      </header>

      <Routes>
        <Route path="/" element={<Leaderboard/>}/>
      </Routes>
    </BrowserRouter>
  )
}

export default App
