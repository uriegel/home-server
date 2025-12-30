import express from "express"
import morgan from "morgan"
import cors from "cors"
import "functional-extensions"
import { router } from "./routes"

console.log("Server fährt hoch...")

const PORT = process.env.PORT || 9865

const app = express()
app.use(cors())
app.use(morgan(":method :url :status :res[content-length] - :response-time ms"))
app.use(router)

const server = app.listen(PORT, () => console.log(`Listening on ${PORT}`))

process.on('SIGTERM', shutdown)
process.on('SIGINT', shutdown)

function shutdown() {
    console.log('SIGTERM signal received: closing HTTP server')
    server.close(() => console.log('Server herunter gefahren'))
}


