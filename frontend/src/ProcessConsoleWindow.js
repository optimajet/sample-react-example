import ProcessConsole from './ProcessConsole';
import {Container} from 'rsuite';

const ProcessConsoleWindow = ({processId}) => {
    return <Container style={{maxHeight: 900, 'overflow-y': 'scroll'}}>
        <b>Process console</b>
        <ProcessConsole processId={processId}/>
    </Container>
}
export default ProcessConsoleWindow;