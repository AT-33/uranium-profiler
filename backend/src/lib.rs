use std::time::Instant;

#[cfg(test)]
mod tests;

pub static mut START_TIME: Option<Instant> = None;
pub static mut EVENTS: Vec<(u128, &'static str, EventType)> = Vec::new();

#[derive(Debug, Copy, Clone)]
pub enum EventType {
    Begin,
    End,
}

impl PartialEq for EventType {
    fn eq(&self, other: &Self) -> bool {
        let a = match self {
            EventType::Begin => 0,
            EventType::End => 1,
        };
        let b = match other {
            EventType::Begin => 0,
            EventType::End => 1,
        };
        a == b
    }
}

#[inline]
pub fn profiler_event(name: &'static str, event_type: EventType) {
    unsafe {
        let timestamp = START_TIME
            .expect("Profiler must be initialized with initialize_profiler()")
            .elapsed()
            .as_micros();
        EVENTS.push((timestamp, name, event_type));
    }
}

pub fn initialize_profiler() {
    unsafe {
        START_TIME = Some(Instant::now());
    }
}
