import {Container, Timeline} from 'rsuite';
import {useProcessesConsoleContext} from "./ProcessesConsoleContext";
import CircleIcon from '@rsuite/icons/legacy/Circle';
import ReactMarkdown from 'react-markdown';

const ProcessConsoleTimeline = ({processId, align = 'left'}) => {
    const processesConsoleContext = useProcessesConsoleContext();
    const consoleLines = processesConsoleContext[processId] ?? [];
    const timeLineItems = consoleLines.map((ti, i) => <Timeline.Item key={i}
        dot={i === 0 ? <CircleIcon style={{color: '#15b215'}}/> : <CircleIcon/>}>
        <p>{`${ti.date.toLocaleDateString()} ${ti.date.toLocaleTimeString()}`}</p>
        {ti.message.split('\n').map((s, index) => {
            return <div key={index}><ReactMarkdown>{s}</ReactMarkdown></div>
        })}
    </Timeline.Item>)
    return <Timeline align={align}>{timeLineItems}</Timeline>
}

const ProcessConsole = ({processId}) => {
    return <Container style={{maxHeight: 900, overflowY: 'scroll'}}>
        <b>Process console</b>
        <ProcessConsoleTimeline processId={processId}/>
    </Container>
}
export default ProcessConsole;
