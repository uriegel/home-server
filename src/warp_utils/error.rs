
#[derive(Debug)]
pub struct Error {
}

impl From<std::io::Error> for Error {
    fn from(_error: std::io::Error) -> Self {
        Error {
        }
    }
}

impl warp::reject::Reject for Error {
}
