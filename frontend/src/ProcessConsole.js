import {Timeline} from 'rsuite';
import {ProcessConsoleProvider, useProcessConsoleContext} from './ProcessConsoleContext';
import CircleIcon from '@rsuite/icons/legacy/Circle';

const ProcessConsoleTimeLine = ({align = 'left'}) => {
    const consoleLines = useProcessConsoleContext();
    const timeLineItems = consoleLines.map((ti, i) => <Timeline.Item
        dot={i === 0 ? <CircleIcon style={{color: '#15b215'}}/> : <CircleIcon/>}>
        <p>{`${ti.date.toLocaleDateString()} ${ti.date.toLocaleTimeString()}`}</p>
        <p>{ti.message.split('\n').map(s => {
            return <><span>{s}</span> <br/></>
        })}</p>
    </Timeline.Item>)
    return <Timeline align={align}>{timeLineItems}</Timeline>
}

const ProcessConsole = ({processId, align = 'left'}) => {
    return <ProcessConsoleProvider processId={processId}>
        <ProcessConsoleTimeLine align={align}/>
    </ProcessConsoleProvider>
}

export default ProcessConsole;