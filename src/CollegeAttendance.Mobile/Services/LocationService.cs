namespace CollegeAttendance.Mobile.Services;

public class LocationService
{
    private CancellationTokenSource? _cts;

    public async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                    return null;
            }

            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request, _cts.Token);
            return location;
        }
        catch (PermissionException)
        {
            return null;
        }
        catch (FeatureNotEnabledException)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    public static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        var loc1 = new Location(lat1, lon1);
        var loc2 = new Location(lat2, lon2);
        return Location.CalculateDistance(loc1, loc2, DistanceUnits.Kilometers) * 1000;
    }
}
