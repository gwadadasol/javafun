import org.example.Main;
import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.*;

public class MainTest {
    @Test
    void sample_test_works(){
        assertEquals(2,1+1);
    }

    @Test
    void greeting_defaultsToWorld_whenNoArgs(){
        assertEquals("Hello world!", Main.gretting(new String[]{}));
    }

    @Test
    void greeting_useFirstArg_whenProvded(){
        assertEquals("Hello world!", Main.gretting(new String[]{}));
    }
}
